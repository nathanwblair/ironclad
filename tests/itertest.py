
from tests.utils.runtest import makesuite, run
from tests.utils.testcase import TestCase

from tests.utils.memory import CreateTypes

from System import IntPtr

from Ironclad import Python25Mapper

class IterationTest(TestCase):
    
    def testPyObject_GetIter_Success(self):
        mapper = Python25Mapper()
        deallocTypes = CreateTypes(mapper)
        
        testList = [1, 2, 3]
        listPtr = mapper.Store(testList)
        iterPtr = mapper.PyObject_GetIter(listPtr)
        iter = mapper.Retrieve(iterPtr)
        self.assertEquals([x for x in iter], testList, "bad iterator")
            
        mapper.Dispose()
        deallocTypes()
    
    
    def testPyObject_GetIter_Failure(self):
        mapper = Python25Mapper()
        deallocTypes = CreateTypes(mapper)
        
        testObj = object()
        objPtr = mapper.Store(testObj)
        iterPtr = mapper.PyObject_GetIter(objPtr)
        self.assertEquals(iterPtr, IntPtr.Zero, "returned iterator inappropriately")
        self.assertNotEquals(mapper.LastException, None, "failed to set exception")
        
        def Raise():
            raise mapper.LastException
        try:
            Raise()
        except TypeError, e:
            self.assertEquals(str(e), "PyObject_GetIter: object is not iterable", "bad message")
        else:
            self.fail("wrong exception")
                
        mapper.Dispose()
        deallocTypes()
    
    
    def testPyIter_Next_Success(self):
        mapper = Python25Mapper()
        deallocTypes = CreateTypes(mapper)
        
        testList = [0, 1, 2]
        listPtr = mapper.Store(testList)
        iterPtr = mapper.PyObject_GetIter(listPtr)
        
        for i in range(3):
            itemPtr = mapper.PyIter_Next(iterPtr)
            self.assertEquals(mapper.Retrieve(itemPtr), i, "got wrong object back")
            self.assertEquals(mapper.RefCount(itemPtr), 2, "failed to incref")
            mapper.DecRef(itemPtr)
        
        noItemPtr = mapper.PyIter_Next(iterPtr)
        self.assertEquals(noItemPtr, IntPtr.Zero, "failed to stop iterating")
            
        mapper.Dispose()
        deallocTypes()
    
    
    def testPyIter_Next_NotAnIterator(self):
        mapper = Python25Mapper()
        deallocTypes = CreateTypes(mapper)
        
        notIterPtr = mapper.Store(object())
        self.assertEquals(mapper.PyIter_Next(notIterPtr), IntPtr.Zero, "bad return")
        self.assertNotEquals(mapper.LastException, None, "failed to set exception")
        
        def Raise():
            raise mapper.LastException
        try:
            Raise()
        except TypeError, e:
            self.assertEquals(str(e), "PyIter_Next: object is not an iterator", "bad message")
        else:
            self.fail("wrong exception")
            
        mapper.Dispose()
        deallocTypes()
    
    
    def testPyIter_Next_ExplodingIterator(self):
        mapper = Python25Mapper()
        deallocTypes = CreateTypes(mapper)
        
        class BorkedException(Exception):
            pass
        def GetNext():
            raise BorkedException("Release the hounds!")
        explodingIterator = (GetNext() for _ in range(3))
        
        iterPtr = mapper.Store(explodingIterator)
        self.assertEquals(mapper.PyIter_Next(iterPtr), IntPtr.Zero, "bad return")
        self.assertNotEquals(mapper.LastException, None, "failed to set exception")
        
        def Raise():
            raise mapper.LastException
        try:
            Raise()
        except BorkedException, e:
            self.assertEquals(str(e), "Release the hounds!", "unexpected message")
        else:
            self.fail("wrong exception")
            
        mapper.Dispose()
        deallocTypes()

class SequenceIterationTest(TestCase):
    
    def testPySeqIter_New(self):
        mapper = Python25Mapper()
        deallocTypes = CreateTypes(mapper)
    
        class SomeSequence(object):
            def __getitem__(self, i):
                if i < 5: return i * 10
                raise IndexError()
            def __len__(self):
                return 5
    
        seqs = ([5, 4, 3], (2, 1, 0, -1), SomeSequence(), 'rawr!')
        for seq in seqs:
            seqPtr = mapper.Store(seq)
            iterPtr = mapper.PySeqIter_New(seqPtr)
            _iter = mapper.Retrieve(iterPtr)
            for item in seq:
                self.assertEquals(_iter.next(), item)
            mapper.DecRef(iterPtr)
            mapper.DecRef(seqPtr)
    
        notseqs = (3, -2.5e5, object, object())
        for notseq in notseqs:
            notseqPtr = mapper.Store(notseq)
            mapper.LastException = None
            self.assertEquals(mapper.PySeqIter_New(notseqPtr), IntPtr.Zero)
            def KindaConvertError():
                raise mapper.LastException
            self.assertRaises(TypeError, KindaConvertError)
    
        mapper.Dispose()
        deallocTypes()


suite = makesuite(
    IterationTest,
    SequenceIterationTest,
)
if __name__ == '__main__':
    run(suite)
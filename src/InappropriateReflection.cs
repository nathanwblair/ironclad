using System;
using System.IO;
using System.Reflection;

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;

namespace Ironclad
{
    internal class InappropriateReflection
    {
        public static FileStream
        StreamFromPythonFile(PythonFile pyFile)
        {
            FieldInfo streamField = (FieldInfo)(pyFile.GetType().GetMember(
                "_stream", BindingFlags.NonPublic | BindingFlags.Instance)[0]);
            return (FileStream)streamField.GetValue(pyFile);
        }
        
        public static PythonContext
        PythonContextFromEngine(ScriptEngine engine)
        {
            FieldInfo _languageField = (FieldInfo)(engine.GetType().GetMember(
                "_language", BindingFlags.NonPublic | BindingFlags.Instance)[0]);
            return (PythonContext)_languageField.GetValue(engine);
        }
        
        public static PythonType
        PythonTypeFromDictProxy(DictProxy proxy)
        {
            FieldInfo _typeField = (FieldInfo)(proxy.GetType().GetMember(
                "_dt", BindingFlags.NonPublic | BindingFlags.Instance)[0]);
            return (PythonType)_typeField.GetValue(proxy);
        }

    }
}
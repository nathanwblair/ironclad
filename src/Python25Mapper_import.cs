using System;
using System.Reflection;
using System.Collections.Generic;

using IronPython.Hosting;
using IronPython.Runtime;
using IronPython.Runtime.Types;

using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Runtime;

namespace Ironclad
{
    public partial class Python25Mapper : Python25Api
    {
        public override IntPtr
        PyImport_ImportModule(string name)
        {
            return this.Store(this.Import(name));
        }

        public override IntPtr
        PyImport_Import(IntPtr namePtr)
        {
            string name = (string)this.Retrieve(namePtr);
            return this.Store(this.Import(name));
        }

        public override IntPtr
        PyImport_AddModule(string name)
        {
            this.CreateModulesContaining(name);
            return this.Store(this.GetModule(name));
        }

        public void 
        LoadModule(string path, string name)
        {
            this.importName = name;
            this.importer.Load(path);
            this.importName = "";
        }

        public object
        Import(string name)
        {
            object module = this.GetModule(name);
            if (module != null)
            {
                return module;
            }
            
            this.importName = name;
            this.ExecInModule(String.Format("import {0}", name), this.scratchModule);
            this.importName = "";
    
            return this.GetModule(name);
        }
        
        private Scope
        CreateModule(string name)
        {
            if (this.GetModule(name) == null)
            {
                PythonDictionary __dict__ = new PythonDictionary();
                __dict__["__name__"] = name;
                Scope module = new Scope(__dict__);
                this.AddModule(name, module);
                return module;
            }
            return null;
        }

        private void
        CreateModulesContaining(string name)
        {
            this.CreateModule(name);
            object innerScope = this.GetModule(name);

            int lastDot = name.LastIndexOf('.');
            if (lastDot != -1)
            {
                this.CreateModulesContaining(name.Substring(0, lastDot));
                Scope outerScope = (Scope)this.GetModule(name.Substring(0, lastDot));
                ScopeOps.__setattr__(outerScope, name.Substring(lastDot + 1), innerScope);
            }
        }

        public void
        PerpetrateNumpyFixes()
        {
            if (this.appliedNumpyHack)
            {
                return;
            }
            this.appliedNumpyHack = true;
            
            Console.WriteLine("Detected numpy import");
            Console.WriteLine("  faking out modules: nosetester, parser, mmap, urllib2, ctypes");
            this.CreateModule("parser");
            this.CreateModule("mmap");

            Scope urllib2 = this.CreateModule("urllib2");
            ScopeOps.__setattr__(urllib2, "urlopen", new Object());
            ScopeOps.__setattr__(urllib2, "URLError", new Object());

            Scope nosetester = this.CreateModule("numpy.testing.nosetester");
            PythonDictionary NoseTesterDict = new PythonDictionary();
            NoseTesterDict["bench"] = NoseTesterDict["test"] = "This has been patched and broken by ironclad";
            PythonType NoseTesterClass = new PythonType(this.scratchContext, "NoseTester", new PythonTuple(), NoseTesterDict);
            ScopeOps.__setattr__(nosetester, "NoseTester", NoseTesterClass);
            ScopeOps.__setattr__(nosetester, "import_nose", new Object());
            ScopeOps.__setattr__(nosetester, "run_module_suite", new Object());
            ScopeOps.__setattr__(nosetester, "get_package_name", new Object());
          
            // ctypeslib specifically handles ctypes being None
            Scope sys = this.python.SystemState;
            PythonDictionary modules = (PythonDictionary)ScopeOps.__getattribute__(sys, "modules");
            modules["ctypes"] = null;

            // this should be fixed in ipy at some point
            ScopeOps.__setattr__(sys, "__displayhook__",
                ScopeOps.__getattribute__(sys, "displayhook"));
        }
    }
}

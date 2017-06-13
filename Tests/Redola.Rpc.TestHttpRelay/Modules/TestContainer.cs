using System;
using System.Collections.Generic;
using Happer;
using Happer.Http;

namespace Redola.Rpc.TestHttpRelay
{
    public class TestContainer : IModuleContainer
    {
        private Dictionary<string, Module> _modules = new Dictionary<string, Module>();

        public TestContainer()
        {
        }

        public void AddModule(Module module)
        {
            _modules.Add(module.GetType().FullName, module);
        }

        public IEnumerable<Module> GetAllModules()
        {
            return _modules.Values;
        }

        public Module GetModule(Type moduleType)
        {
            return _modules[moduleType.FullName];
        }
    }
}

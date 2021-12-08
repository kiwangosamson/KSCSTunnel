using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using TA.UgconnectTunnel.ModuleBaseClasses;

namespace TA.UgconnectTunnel
{
    public class PluginLoader
    {
        private Role _Role;
        private List<Assembly> _Assemblys = new List<Assembly>();
        //private List<Type> _ClientConnectors = new List<Type>();
        //private List<Type> _ServerConnectors = new List<Type>();
        private List<Type> _ClientAuthenticators = new List<Type>();
        private List<Type> _ServerAuthenticators = new List<Type>();

        public PluginLoader(Role Role) { _Role = Role; }

        public void Load(string File)
        {
            Assembly assembly = LoadAssembly(File);
            _Assemblys.Add(assembly);

            foreach (Type type in assembly.GetTypes())
                if (type.IsClass && !type.IsAbstract)
                {
                    //Connector plugin support here

                    if (_Role == Role.Client)
                        if (type.IsSubclassOf(typeof(ClientAuthenticator)))
                            _ClientAuthenticators.Add(type);
                        else if (_Role == Role.Server)
                            if (type.IsSubclassOf(typeof(ServerAuthenticator)))
                                _ServerAuthenticators.Add(type);
                            else throw new Exception("Unknown role");
                }
        }

        private static Assembly LoadAssembly(string File)
        {
            using (FileStream fs = new FileStream(File, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                byte[] raw = new byte[fs.Length];
                //TODO Implement signature check
                return Assembly.Load(raw);
            }
        }
    }
}

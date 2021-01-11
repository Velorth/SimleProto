using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace SimpleProto.Scripting
{
    /// <summary>
    /// ScriptLibrary is a container for functions available for visual scripting.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public static class ScriptLibrary
    {
        private static readonly Type[] NoArgs = { };
        private static readonly Dictionary<string, FunctionInfo> _functionsMap =  new Dictionary<string, FunctionInfo>();
        private static readonly List<FunctionInfo> _functions = new List<FunctionInfo>();

        /// <summary>
        /// Gets collection of all registred functions
        /// </summary>
        public static IReadOnlyList<FunctionInfo> Functions
        {
            get { return _functions; }
        }

        /// <summary>
        /// Registers new function in library.
        /// </summary>
        /// <param name="name">Name of the function.</param>
        /// <param name="func">Function to register.</param>
        public static void RegisterAction(string name, Action func)
        {
            var functionInfo = new FunctionInfo
            {
                Name = name,
                ReturnType = typeof(void),
                ArgTypes = NoArgs,
                Function = environment =>
                {
                    func();
                }
            };
            _functionsMap.Add(name, functionInfo);
            _functions.Add(functionInfo);
        }

        /// <summary>
        /// Registers new function in library.
        /// </summary>
        /// <param name="name">Name of the function.</param>
        /// <param name="func">Function to register.</param>
        public static void RegisterAction<TArg0>(string name, [NotNull]Action<TArg0> func) where TArg0 : UnityEngine.Object
        {
            RegisterFunction(new FunctionInfo
            {
                Name = name,
                ReturnType = typeof(void),
                ArgTypes = new[] { typeof(TArg0) },
                Function = environment =>
                {
                    var arg = environment.PopObject<TArg0>();

                    func(arg);
                }
            });
        }

        public static void RegisterFunction(FunctionInfo functionInfo)
        {
            _functionsMap.Add(functionInfo.Name, functionInfo);
            _functions.Add(functionInfo);
        }

        /// <summary>
        /// Registers new function in library.
        /// </summary>
        /// <param name="name">Name of the function.</param>
        /// <param name="func">Function to register.</param>
        public static void RegisterAction<TArg0, TArg1>(string name, [NotNull]Action<TArg0, TArg1> func) where TArg0 : UnityEngine.Object where TArg1 : UnityEngine.Object
        {
            var functionInfo = new FunctionInfo
            {
                Name = name,
                ReturnType = typeof(void),
                ArgTypes = new[] { typeof(TArg0), typeof(TArg1) },
                Function = environment =>
                {
                    var arg1 = environment.PopObject<TArg1>();
                    var arg0 = environment.PopObject<TArg0>();
                    func(arg0, arg1);
                }
            };
            _functionsMap.Add(name, functionInfo);
            _functions.Add(functionInfo);
        }

        /// <summary>
        /// Registers new function in library.
        /// </summary>
        /// <param name="name">Name of the function.</param>
        /// <param name="func">Function to register.</param>
        public static void RegisterBoolFunction<TArg0>(string name, [NotNull] Predicate<TArg0> func) where TArg0 : UnityEngine.Object
        {
            var functionInfo = new FunctionInfo
            {
                Name = name,
                ReturnType = typeof(bool),
                ArgTypes = new []{ typeof(TArg0) },
                Function = environment =>
                {
                    var arg = environment.PopObject<TArg0>();
                    environment.Push(func(arg));
                }
            };
            _functionsMap.Add(name, functionInfo);
            _functions.Add(functionInfo);
        }

        /// <summary>
        /// Gets a registered function with given name
        /// </summary>
        /// <param name="name">Name of the function</param>
        /// <returns>Registered function with given name or null otherwise</returns>
        [CanBeNull]
        public static FunctionInfo FindFunction(string name)
        {
            FunctionInfo function;
            _functionsMap.TryGetValue(name, out function);
            return function;
        }
    }
}
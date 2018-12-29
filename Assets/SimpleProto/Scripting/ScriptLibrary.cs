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
            _functionsMap.Add(name, new FunctionInfo
            {
                Name = name,
                ReturnType = typeof(void),
                ArgTypes = NoArgs,
                Function = parameters =>
                {
                    func();
                    return null;
                }
            });
        }

        /// <summary>
        /// Registers new function in library.
        /// </summary>
        /// <param name="name">Name of the function.</param>
        /// <param name="func">Function to register.</param>
        public static void RegisterAction<TArg0>(string name, [NotNull]Action<TArg0> func)
        {
            _functionsMap.Add(name, new FunctionInfo
            {
                Name = name,
                ReturnType = typeof(void),
                ArgTypes = new [] { typeof(TArg0) },
                Function = parameters =>
                {
                    func((TArg0)parameters[0]);
                    return null;
                }
            });
        }

        /// <summary>
        /// Registers new function in library.
        /// </summary>
        /// <param name="name">Name of the function.</param>
        /// <param name="func">Function to register.</param>
        public static void RegisterAction<TArg0, TArg1>(string name, [NotNull]Action<TArg0, TArg1> func)
        {
            _functionsMap.Add(name, new FunctionInfo
            {
                Name = name,
                ReturnType = typeof(void),
                ArgTypes = new[] { typeof(TArg0), typeof(TArg1) },
                Function = parameters =>
                {
                    func((TArg0)parameters[0], (TArg1)parameters[1]);
                    return null;
                }
            });
        }

        /// <summary>
        /// Registers new function in library.
        /// </summary>
        /// <param name="name">Name of the function.</param>
        /// <param name="func">Function to register.</param>
        public static void RegisterFunction<TArg0, TResult>(string name, [NotNull] Func<TArg0, TResult> func)
        {
            _functionsMap.Add(name, new FunctionInfo
            {
                Name = name,
                ReturnType = typeof(TResult),
                ArgTypes = new []{ typeof(TArg0) },
                Function = parameters =>
                {
                    return func((TArg0)parameters[0]);
                }
            });
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
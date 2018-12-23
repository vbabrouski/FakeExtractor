using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FakeExtractor.Helpers;
using static FakeExtractor.Helpers.Tracer;

namespace FakeExtractor.Strategy.Target
{
    public static class AssemblyTypeDetector
    {
        private static readonly HashSet<AssemblyName> AssemblyNames = new HashSet<AssemblyName>();
        private static readonly HashSet<string> ReferencePaths = new HashSet<string>();

        private static readonly Dictionary<FakesType, HashSet<string>> TypeDefinitions = new Dictionary<FakesType, HashSet<string>>
        {
            { FakesType.Shim, new HashSet<string>() },
            { FakesType.Stub, new HashSet<string>() }
        };

        public static readonly Dictionary<FakesType, HashSet<Type>> DetectedTypes = new Dictionary<FakesType, HashSet<Type>>
        {
            { FakesType.Shim, new HashSet<Type>() },
            { FakesType.Stub, new HashSet<Type>() }
        };

        private static readonly Dictionary<string, Assembly> FakesAssemblies = new Dictionary<string, Assembly>();

        static AssemblyTypeDetector()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var assemblyName = FindAssembly(args.Name);
                return assemblyName == null
                    ? null
                    : LoadAssembly(assemblyName);
            };
        }

        private static AssemblyName FindAssembly(string name)
        {
            var referencePaths = ReferencePaths.ToArray();
            foreach (var referencePath in referencePaths)
            {
                var file = Directory.GetFiles(referencePath, $"{name}.dll", SearchOption.AllDirectories).FirstOrDefault();
                file = file ?? Directory.GetFiles(referencePath, $"{name}.exe", SearchOption.AllDirectories).FirstOrDefault();
                if (file != null)
                {
                    return AssemblyName.GetAssemblyName(file);
                }
            }

            return null;
        }

        public static void AddAssemblySearchPath(string path)
        {
            var files = Directory
                .GetFiles(path, FileExtensions.LibrarySearchPattern, SearchOption.AllDirectories)
                .Where(x=>!x.ContainsOrdinalIgnoreCase("\\Fakes\\"));
            foreach (var file in files)
            {
                var assemblyName = GetAssemblyName(file);
                if (assemblyName != null)
                {
                    AssemblyNames.Add(assemblyName);
                }
            }
        }

        public static void AddTypeDefinitions(IEnumerable<string> definitions)
        {
            foreach (var definition in definitions)
            {
                var baseDefinition = definition.ReplaceOrdinalIgnoreCase(".Fakes.", ".");
                var shimDefinition = baseDefinition.ReplaceOrdinalIgnoreCase(".Shim", ".");
                var stubDefinition = baseDefinition.ReplaceOrdinalIgnoreCase(".Stub", ".");
                if (!baseDefinition.EqualTo(shimDefinition))
                {
                    TypeDefinitions[FakesType.Shim].Add(shimDefinition);
                }
                else if (!baseDefinition.EqualTo(stubDefinition))
                {
                    TypeDefinitions[FakesType.Stub].Add(stubDefinition);
                }
            }
        }

        public static void AddAssembly(string assemblyFile)
        {
            if (!string.IsNullOrWhiteSpace(assemblyFile) &&
                !assemblyFile.EndsWith("Microsoft.QualityTools.Testing.Fakes.dll", StringComparison.OrdinalIgnoreCase))
            {
                var assemblyName = GetAssemblyName(assemblyFile);
                if (assemblyName == null)
                {
                    return;
                }

                if (IsFakeAssembly(assemblyName))
                {
                    assemblyName = GetBaseAssemblyForFakes(assemblyName);
                    if (assemblyName == null)
                    {
                        return;
                    }
                }

                if (!FakesAssemblies.ContainsKey(assemblyName.CodeBase))
                {
                    var assembly = LoadAssembly(assemblyName);
                    if (assembly == null)
                    {
                        return;
                    }

                    FakesAssemblies[assemblyName.CodeBase] = assembly;
                }

                ProcessAssemblyTypes(FakesAssemblies[assemblyName.CodeBase]);
            }
        }

        public static void AddReferencePaths(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                if (!Directory.Exists(path))
                {
                    continue;
                }

                if (ReferencePaths.Any(x => path.StartsWithOrdinal(x)))
                {
                    continue;
                }

                var childPath = ReferencePaths.Where(x => x.StartsWithOrdinal(path)).ToArray();
                foreach (var item in childPath)
                {
                    ReferencePaths.Remove(item);
                }

                ReferencePaths.Add(path);
            }
        }

        private static AssemblyName GetBaseAssemblyForFakes(AssemblyName assemblyName)
        {
            var name = assemblyName.Name;
            var replace = $".{assemblyName.Version}.Fakes";
            var baseName = name.Replace(replace, string.Empty);
            var baseAssemblyName = AssemblyNames.FirstOrDefault(x => x.Name.EqualTo(baseName) && x.Version == assemblyName.Version);
            return baseAssemblyName ?? TryGetAssemblyNameFromSystem(baseName);
        }

        private static AssemblyName TryGetAssemblyNameFromSystem(string name)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var assembly = assemblies.FirstOrDefault(x => x.GetName().Name.EqualTo(name));
            assembly = assembly ?? Assembly.LoadWithPartialName(name);
            var assemblyName = assembly?.GetName();
            if (assemblyName != null)
            {
                AssemblyNames.Add(assemblyName);
            }

            return assemblyName;
        }

        private static bool IsFakeAssembly(AssemblyName assemblyName)
        {
            return assemblyName?.Name.EndsWithOrdinalIgnoreCase(".Fakes") ?? false;
        }

        private static void ProcessAssemblyTypes(Assembly assembly)
        {
            foreach (var fakesType in TypeDefinitions.Keys)
            {
                foreach (var probeType in TypeDefinitions[fakesType])
                {
                    var type = assembly.GetType(probeType);
                    if (type != null)
                    {
                        DetectedTypes[fakesType].Add(type);
                    }
                }
            }
            /*
            var types = GetAssemblyTypes(assembly);
            foreach (var type in types)
            {
                AssignType(type);
            }
            */
        }

        private static void AssignType(Type type)
        {
            var fullName = type.FullName;
            if (TypeDefinitions[FakesType.Shim].Contains(fullName))
            {
                DetectedTypes[FakesType.Shim].Add(type);
                return;
            }

            if (TypeDefinitions[FakesType.Stub].Contains(fullName))
            {
                DetectedTypes[FakesType.Stub].Add(type);
            }
        }

        private static IEnumerable<Type> GetAssemblyTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetExportedTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                return exception.Types.Where(t => t != null);
            }
        }

        private static Assembly LoadAssembly(AssemblyName assemblyName)
        {
            try
            {
                return Assembly.Load(assemblyName);
            }
            catch (Exception exception)
            {
                Error(exception, $"Error loading assembly '{assemblyName}'");
            }

            return null;
        }

        private static AssemblyName GetAssemblyName(string assemblyFile)
        {
            try
            {
                return AssemblyName.GetAssemblyName(assemblyFile);
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                var name = Path.GetFileNameWithoutExtension(assemblyFile);
                return FindAssembly(name);
            }
            catch (Exception exception)
            {
                Error(exception, $"Error get assembly name '{assemblyFile}'");
            }

            return null;
        }
    }
}
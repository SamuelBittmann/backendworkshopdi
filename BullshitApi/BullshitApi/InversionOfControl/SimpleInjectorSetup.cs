using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using SimpleInjector;

namespace Dg.InversionOfControl
{
    public static class SimpleInjectorSetup
    {
        public static void InitContainer(
            ScopedLifestyle scopedLifestyle,
            Container container,
            params SimpleInjectorConfigBase[] configs)
        {
            InitContainerImpl(scopedLifestyle, container, null, configs);
        }

        public static Container InitContainer<TConfig>(
            Lifestyle lifestyle) where TConfig : SimpleInjectorConfigBase, new()
        {
            var container = new Container();
            InitContainerImpl<TConfig>(lifestyle, container);
            return container;
        }

        public static void InitContainer<TConfig>(
            Lifestyle lifestyle,
            Container container) where TConfig : SimpleInjectorConfigBase, new()
        {
            InitContainerImpl<TConfig>(
                lifestyle,
                container);
        }

        private static void InitContainerImpl(
            Lifestyle lifestyle,
            Container container,
            Action<Container> yetAnotherUglyHackForPortalSystemBecauseWeDidNotSeparateOurApplicationsYet,
            SimpleInjectorConfigBase[] configs)
        {
            ProcessConfigsAndInitializeContainer(
                lifestyle,
                container,
                yetAnotherUglyHackForPortalSystemBecauseWeDidNotSeparateOurApplicationsYet,
                configs);
        }

        private static void InitContainerImpl<TConfig>(
            Lifestyle lifestyle,
            Container container) where TConfig : SimpleInjectorConfigBase, new()
        {
            ProcessConfigsAndInitializeContainer(
                lifestyle,
                container,
                yetAnotherUglyHackForPortalSystemBecauseWeDidNotSeparateOurApplicationsYet: null,
                configs: new[] { new TConfig() });
        }

        private static void ProcessConfigsAndInitializeContainer(
            Lifestyle lifestyle,
            Container container,
            Action<Container> yetAnotherUglyHackForPortalSystemBecauseWeDidNotSeparateOurApplicationsYet,
            SimpleInjectorConfigBase[] configs)
        {

            if (lifestyle is ScopedLifestyle scopedLifestyle && container.Options.DefaultScopedLifestyle == null)
            {
                container.Options.DefaultScopedLifestyle = scopedLifestyle;
                container.Options.DefaultLifestyle = scopedLifestyle;
            }

            yetAnotherUglyHackForPortalSystemBecauseWeDidNotSeparateOurApplicationsYet?.Invoke(container);

            var sortedConfigs = SimpleInjectorConfigUtils.GetAllConfigsAndRequiredConfigsSorted(configs);
            var assemblies = SimpleInjectorConfigUtils.GetAssemblies(sortedConfigs);
            var validationFunctions = SimpleInjectorConfigUtils.GetValidationFunctions(sortedConfigs);

            foreach (var config in sortedConfigs)
            {
                config.InitSpecialCases(container, assemblies, validationFunctions);
            }

            // RegisterTypes() needs the other types to be registered, do don't run it in parallel with the other registering methods
            RegisterTypes(
                container,
                sortedConfigs,
                assemblies,
                validationFunctions);
        }



        private static void RegisterTypes(
            Container container,
            IReadOnlyList<SimpleInjectorConfigBase> sortedConfigs,
            IReadOnlySet<Assembly> assemblies,
            IReadOnlyList<Func<Type, bool>> validationFunctions)
        {
            var collectionTypes = SimpleInjectorConfigUtils.GetCollectionTypes(sortedConfigs);
            var blackListedTypes = SimpleInjectorConfigUtils.GetBlackListedTypes(sortedConfigs);

            RegisterCollectionTypes(
                container: container,
                validationFunctions: validationFunctions,
                assemblies: assemblies,
                collectionTypes: collectionTypes,
                blackListedTypes: blackListedTypes);

            RegisterNonCollectionTypes(
                container: container,
                validationFunctions: validationFunctions,
                typeSuffixRegistrations: SimpleInjectorConfigUtils.GetSuffixRegistrations(sortedConfigs),
                additionalTypes: SimpleInjectorConfigUtils.GetAdditionalTypes(sortedConfigs),
                derivedTypes: SimpleInjectorConfigUtils.GetDerivedTypes(sortedConfigs, assemblies),
                alreadyRegisteredTypes: GetAlreadyRegisteredTypes(container),
                collectionTypes: collectionTypes,
                blackListedTypes: blackListedTypes,
                assemblies: assemblies,
                allowInternalTypes: sortedConfigs.Any(c => c.AllowInternalTypes));
        }

        private static IReadOnlyList<Type> GetAlreadyRegisteredTypes(Container container)
        {
            var registrations = container.GetCurrentRegistrations();

            var alreadyRegisteredTypes = registrations
                .Select(r => r.ServiceType)
                .Union(registrations.Select(r => r.Registration.ImplementationType));

            return alreadyRegisteredTypes.ToList();
        }

        private static void RegisterCollectionTypes(
            Container container,
            IReadOnlyList<Func<Type, bool>> validationFunctions,
            IReadOnlySet<Assembly> assemblies,
            IReadOnlyList<Type> collectionTypes,
            IReadOnlyList<Type> blackListedTypes)
        {
            foreach (var collectionType in collectionTypes)
            {
                if (blackListedTypes.None(blt => IsCollectionOfGivenType(typeToCheck: blt, typeInCollection: collectionType))
                    && validationFunctions.All(vf => vf(collectionType)))
                {
                    container.Collection.Register(collectionType, assemblies);
                }
            }
        }

        private static bool IsCollectionOfGivenType(Type typeToCheck, Type typeInCollection)
        {
            const string interfaceName = "IEnumerable`1";
            var ienumerableInterface = typeToCheck.Name == interfaceName ? typeToCheck : typeToCheck.GetInterface(interfaceName);
            if (ienumerableInterface is null)
            {
                return false;
            }

            return ienumerableInterface.GenericTypeArguments[0] == typeInCollection;
        }

        private static void RegisterNonCollectionTypes(
            Container container,
            IReadOnlyList<Func<Type, bool>> validationFunctions,
            IReadOnlySet<string> typeSuffixRegistrations,
            IReadOnlyList<Type> additionalTypes,
            IReadOnlyList<Type> derivedTypes,
            IReadOnlyList<Type> alreadyRegisteredTypes,
            IReadOnlyList<Type> collectionTypes,
            IReadOnlyList<Type> blackListedTypes,
            IReadOnlySet<Assembly> assemblies,
            bool allowInternalTypes)
        {
            var autoDiscoverTypes = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => typeSuffixRegistrations.Any(suffix => t.Name.EndsWithCaseInsensitive(suffix)))
                .ToList();

            var distinctTypes = SimpleInjectorImplementationTypeService.GetDistinctImplementationTypesToRegister(
                autoDiscoverTypes: autoDiscoverTypes,
                additionalTypes: additionalTypes,
                derivedTypes: derivedTypes,
                alreadyRegisteredTypes: alreadyRegisteredTypes,
                collectionTypes: collectionTypes,
                blackListedTypes: blackListedTypes,
                validationFunctions: validationFunctions,
                allowInternalTypes: allowInternalTypes);

            var failedRegistrations = new List<Tuple<Type, Exception>>();
            foreach (var distinctType in distinctTypes)
            {
                try
                {
                    container.Register(distinctType.Key, distinctType.Value);
                }
                catch (Exception e)
                {
                    failedRegistrations.Add(Tuple.Create(distinctType.Key, e));
                }
            }

            if (failedRegistrations.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Failed to register the following types. Add this string to the blacklist: "
                    + failedRegistrations.Select(r => $"typeof({r.Item1.FullName}),").Concat(Environment.NewLine)
                    + Environment.NewLine
                    + failedRegistrations.Select(r => r.Item2.Message).Concat(Environment.NewLine));

            }

            var warnings = SimpleInjectorImplementationTypeService.GetRegistrationWarnings(
                autoDiscoverTypes: autoDiscoverTypes,
                additionalTypes: additionalTypes,
                derivedTypes: derivedTypes,
                blackListedTypes: blackListedTypes,
                collectionTypes: collectionTypes);

            if (warnings.Count > 0)
            {
                var warningsStr = string.Join(Environment.NewLine, warnings);
                throw new InvalidOperationException("You have configuration errors for simple injector: " + Environment.NewLine + warningsStr);
            }

        }
    }
}
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dg.InversionOfControl
{
    public abstract class SimpleInjectorConfigBase
    {
        public abstract ISet<Type> RequiredConfigTypes { get; }

        public virtual ISet<Type> BlacklistedTypes => new HashSet<Type>();

        public virtual bool IsTypeValidForContainerRegistration(Type type) => true;

        public virtual void InitSpecialCases(
            Container container,
            ISet<Assembly> assemblies,
            IReadOnlyList<Func<Type, bool>> validFunctions)
        { }

        public virtual ISet<Type> AdditionalTypes => new HashSet<Type>();
        public virtual ISet<Type> TypesToRegisterDerivedTypes => new HashSet<Type>();
        public virtual ISet<Type> TypesToRegisterAsCollection => new HashSet<Type>();

        public virtual ISet<string> TypeSuffixRegistrations => new HashSet<string>{
            // Please add only very general suffixes here and not specific stuff (like things that are only valid in UI like controller)
            "repository",
            "service",
            "batch",
            "factory"};

        public virtual bool AllowInternalTypes => false;

        /// <summary>
        /// This little helper allows to check at compile time that the given type derives from SimpleInjectorConfigBase instead of at
        /// runtime which would be the case with typeof().
        /// </summary>
        protected Type GetConfigType<TConfig>() where TConfig : SimpleInjectorConfigBase, new()
        {
            return typeof(TConfig);
        }
    }
}

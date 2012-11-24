namespace Appccelerate.Bootstrapper.Specification.Dummies
{
    using System.Collections.Generic;

    using Appccelerate.Bootstrapper.Syntax;

    public class CustomExtensionStrategyNew : AbstractStrategyWithNewSyntax<ICustomExtension>
    {
        public int RunConfigurationInitializerAccessCounter
        {
            get;
            private set;
        }

        public int ShutdownConfigurationInitializerAccessCounter
        {
            get;
            private set;
        }

        protected override void DefineRunSyntax(ISyntaxBuilderNew<ICustomExtension> builder)
        {
            builder
                .Execute(() => CustomExtensionBase.DumpAction("CustomRun"))
                .Execute(extension => extension.Start())
                .Execute(() => this.RunInitializeConfiguration(), (extension, dictionary) => extension.Configure(dictionary))
                .Execute(extension => extension.Initialize())
                .Execute(() => "RunTest", (extension, ctx) => extension.Register(ctx));
        }

        protected override void DefineShutdownSyntax(ISyntaxBuilderNew<ICustomExtension> syntax)
        {
            syntax
                .Execute(() => CustomExtensionBase.DumpAction("CustomShutdown"))
                .Execute(() => "ShutdownTest", (extension, ctx) => extension.Unregister(ctx))
                .Execute(() => this.ShutdownInitializeConfiguration(), (extension, dictionary) => extension.DeConfigure(dictionary))
                .Execute(extension => extension.Stop());
        }

        private IDictionary<string, string> RunInitializeConfiguration()
        {
            this.RunConfigurationInitializerAccessCounter++;

            return new Dictionary<string, string> { { "RunTest", "RunTestValue" } };
        }

        private IDictionary<string, string> ShutdownInitializeConfiguration()
        {
            this.ShutdownConfigurationInitializerAccessCounter++;

            return new Dictionary<string, string> { { "ShutdownTest", "ShutdownTestValue" } };
        }
    }
}
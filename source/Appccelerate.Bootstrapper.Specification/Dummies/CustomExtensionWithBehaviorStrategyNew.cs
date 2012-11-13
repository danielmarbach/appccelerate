namespace Appccelerate.Bootstrapper.Specification.Dummies
{
    using System.Collections.Generic;

    using Appccelerate.Bootstrapper.Behavior;
    using Appccelerate.Bootstrapper.Syntax;

    public class CustomExtensionWithBehaviorStrategyNew : AbstractStrategyWithNewSyntax<ICustomExtension>
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
                .Apply(new Behavior("run first beginning"))
                .Apply(() => new Behavior("run second beginning"))
                .Execute(() => CustomExtensionBase.DumpAction("CustomRun"))
                .Execute(extension => extension.Start())
                .Apply(new Behavior("run first start"))
                .Apply(() => new Behavior("run second start"))
                .Execute(() => this.RunInitializeConfiguration(), (extension, dictionary) => extension.Configure(dictionary))
                .Apply(dictionary => new BehaviorWithConfigurationContext(dictionary, "RunFirstValue", "RunTestValue"))
                .Apply(dictionary => new BehaviorWithConfigurationContext(dictionary, "RunSecondValue", "RunTestValue"))
                .Execute(extension => extension.Initialize())
                .Apply(new Behavior("run first initialize"))
                .Apply(() => new Behavior("run second initialize"))
                .Execute(() => "RunTest", (extension, context) => extension.Register(context))
                .Apply(context => new BehaviorWithStringContext(context, "RunTestValueFirst"))
                .Apply(context => new BehaviorWithStringContext(context, "RunTestValueSecond"))
                .Apply(new Behavior("run first end"))
                .Apply(() => new Behavior("run second end"));
        }

        protected override void DefineShutdownSyntax(ISyntaxBuilderNew<ICustomExtension> builder)
        {
            builder
                .Apply(new Behavior("shutdown first beginning"))
                .Apply(() => new Behavior("shutdown second beginning"))
                .Execute(() => CustomExtensionBase.DumpAction("CustomShutdown"))
                .Execute(() => "ShutdownTest", (extension, ctx) => extension.Unregister(ctx))
                .Apply(context => new BehaviorWithStringContext(context, "ShutdownTestValueFirst"))
                .Apply(context => new BehaviorWithStringContext(context, "ShutdownTestValueSecond"))
                .Execute(() => this.ShutdownInitializeConfiguration(), (extension, dictionary) => extension.DeConfigure(dictionary))
                .Apply(dictionary => new BehaviorWithConfigurationContext(dictionary, "ShutdownFirstValue", "ShutdownTestValue"))
                .Apply(dictionary => new BehaviorWithConfigurationContext(dictionary, "ShutdownSecondValue", "ShutdownTestValue"))
                .Execute(extension => extension.Stop())
                .Apply(new Behavior("shutdown first stop"))
                .Apply(() => new Behavior("shutdown second stop"))
                .Apply(new Behavior("shutdown first end"))
                .Apply(() => new Behavior("shutdown second end"))
                .Apply(new DisposeExtensionBehavior());
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
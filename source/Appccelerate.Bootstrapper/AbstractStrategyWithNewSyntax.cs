namespace Appccelerate.Bootstrapper
{
    using System;

    using Appccelerate.Bootstrapper.Execution;
    using Appccelerate.Bootstrapper.Extension;
    using Appccelerate.Bootstrapper.Reporting;
    using Appccelerate.Bootstrapper.Syntax;

    public abstract class AbstractStrategyWithNewSyntax<TExtension> : IStrategy<TExtension>
        where TExtension : IExtension
    {
        private readonly ISyntaxBuilderNew<TExtension> runSyntaxBuilder;

        private readonly ISyntaxBuilderNew<TExtension> shutdownSyntaxBuilder;

        private bool runSyntaxBuilded;

        private bool shutdownSyntaxBuilded;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractStrategyWithNewSyntax&lt;TExtension&gt;"/> class.
        /// </summary>
        /// <remarks>Uses the default syntax builder.</remarks>
        protected AbstractStrategyWithNewSyntax()
            : this(null /*new SyntaxBuilderNew<TExtension>(new ExecutableFactory<TExtension>())*/, null /* new SyntaxBuilderNew<TExtension>(new ExecutableFactory<TExtension>())*/)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractStrategyWithNewSyntax&lt;TExtension&gt;"/> class.
        /// </summary>
        /// <param name="runSyntaxBuilder">The run syntax builder.</param>
        /// <param name="shutdownSyntaxBuilder">The shutdown syntax builder.</param>
        protected AbstractStrategyWithNewSyntax(ISyntaxBuilderNew<TExtension> runSyntaxBuilder, ISyntaxBuilderNew<TExtension> shutdownSyntaxBuilder)
        {
            this.shutdownSyntaxBuilder = shutdownSyntaxBuilder;
            this.runSyntaxBuilder = runSyntaxBuilder;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AbstractStrategyWithNewSyntax{TExtension}"/> class.
        /// </summary>
        ~AbstractStrategyWithNewSyntax()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
        protected bool IsDisposed { get; private set; }

        /// <inheritdoc />
        /// <remarks>By default creates a SynchronousExecutor{TExtension}</remarks>
        public virtual IExecutor<TExtension> CreateRunExecutor()
        {
            return new SynchronousExecutor<TExtension>();
        }

        /// <inheritdoc />
        /// <remarks>By default creates a SynchronousReverseExecutor{TExtension}</remarks>
        public virtual IExecutor<TExtension> CreateShutdownExecutor()
        {
            return new SynchronousReverseExecutor<TExtension>();
        }

        /// <inheritdoc />
        /// <remarks>By default creates a NullExtensionResolver{TExtension}</remarks>
        public virtual IExtensionResolver<TExtension> CreateExtensionResolver()
        {
            return new NullExtensionResolver<TExtension>();
        }

        /// <inheritdoc />
        /// <remarks>By default creates a ReportingContext</remarks>
        public virtual IReportingContext CreateReportingContext()
        {
            return new ReportingContext();
        }

        /// <inheritdoc />
        public ISyntax<TExtension> BuildRunSyntax()
        {
            this.CheckRunSyntaxNotAlreadyBuilt();

            this.DefineRunSyntax(this.runSyntaxBuilder);

            return this.runSyntaxBuilder;
        }

        /// <inheritdoc />
        public ISyntax<TExtension> BuildShutdownSyntax()
        {
            this.CheckShutdownSyntaxNotAlreadyBuilt();

            this.DefineShutdownSyntax(this.shutdownSyntaxBuilder);

            return this.shutdownSyntaxBuilder;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.IsDisposed && disposing)
            {
                this.IsDisposed = true;
            }
        }

        /// <summary>
        /// Fluently defines the run syntax on the specified builder.
        /// </summary>
        /// <param name="builder">The syntax builder</param>
        protected abstract void DefineRunSyntax(ISyntaxBuilderNew<TExtension> builder);

        /// <summary>
        /// Fluently defines the shutdown syntax on the specified builder.
        /// </summary>
        /// <param name="builder">The syntax builder</param>
        protected abstract void DefineShutdownSyntax(ISyntaxBuilderNew<TExtension> builder);

        private void CheckRunSyntaxNotAlreadyBuilt()
        {
            if (this.runSyntaxBuilded)
            {
                throw new InvalidOperationException("The run syntax can only be acquired once.");
            }

            this.runSyntaxBuilded = true;
        }

        private void CheckShutdownSyntaxNotAlreadyBuilt()
        {
            if (this.shutdownSyntaxBuilded)
            {
                throw new InvalidOperationException("The shutdown syntax can only be acquired once.");
            }

            this.shutdownSyntaxBuilded = true;
        }
    }
}
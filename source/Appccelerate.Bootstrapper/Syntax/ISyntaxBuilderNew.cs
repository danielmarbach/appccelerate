namespace Appccelerate.Bootstrapper.Syntax
{
    /// <summary>
    /// Defines the interface for a syntax builder.
    /// </summary>
    /// <typeparam name="TExtension">The extension.</typeparam>
    public interface ISyntaxBuilderNew<TExtension> : IWithBehaviorNew<TExtension>
        where TExtension : IExtension
    {
    }

    /// <summary>
    /// Defines the interface for a syntax builder without context
    /// </summary>
    /// <typeparam name="TExtension">The type of the extension.</typeparam>
    public interface ISyntaxBuilderWithoutContextNew<TExtension> : ISyntaxBuilderNew<TExtension>
        where TExtension : IExtension
    {
    }

    /// <summary>
    /// Defines the interface for a syntax builder with context.
    /// </summary>
    /// <typeparam name="TExtension">The type of the extension.</typeparam>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    public interface ISyntaxBuilderWithContextNew<TExtension, TContext> : IWithBehaviorOnContextNew<TExtension, TContext>, IWithBehaviorNew<TExtension>
        where TExtension : IExtension
    {
    }
}
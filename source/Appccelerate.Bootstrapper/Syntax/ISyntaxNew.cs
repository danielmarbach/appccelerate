namespace Appccelerate.Bootstrapper.Syntax
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Generic syntax which operates on extensions.
    /// </summary>
    /// <typeparam name="TExtension">The type of the extension.</typeparam>
    public interface ISyntaxNew<TExtension> : ISyntax<TExtension>
        where TExtension : IExtension
    {
    }

    /// <summary>
    /// Execute action syntax.
    /// </summary>
    /// <typeparam name="TExtension">The type of the extension.</typeparam>
    public interface IExecuteActionNew<TExtension> : ISyntaxNew<TExtension>
        where TExtension : IExtension
    {
        /// <summary>
        /// Adds an execution action to the currently built syntax.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>The current syntax builder.</returns>
        IWithBehaviorNew<TExtension> Execute(Expression<Action> action);
    }

    /// <summary>
    /// Execute an action on an extension syntax.
    /// </summary>
    /// <typeparam name="TExtension">The type of the extension.</typeparam>
    public interface IExecuteActionOnExtensionNew<TExtension> : ISyntaxNew<TExtension>
        where TExtension : IExtension
    {
        /// <summary>
        /// Adds an execution action which operates on the extension to the
        /// currently built syntax.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>The current syntax builder.</returns>
        IWithBehaviorNew<TExtension> Execute(Expression<Action<TExtension>> action);
    }

    /// <summary>
    /// Execute an action on an extension with a context syntax.
    /// </summary>
    /// <typeparam name="TExtension">The type of the extension.</typeparam>
    public interface IExecuteActionOnExtensionWithContextNew<TExtension> : ISyntaxNew<TExtension>
        where TExtension : IExtension
    {
        /// <summary>
        /// Adds an context initializer and an execution action which gets
        /// access to the context to the currently built syntax.
        /// </summary>
        /// <typeparam name="TContext">The type of the context.</typeparam>
        /// <param name="initializer">The context initializer.</param>
        /// <param name="action">The action with access to the context.</param>
        /// <returns>
        /// The current syntax builder.
        /// </returns>
        IWithBehaviorOnContextNew<TExtension, TContext> Execute<TContext>(
            Expression<Func<TContext>> initializer, Expression<Action<TExtension, TContext>> action);
    }

    /// <summary>
    /// Fluent definition syntax interface for behaviors.
    /// </summary>
    /// <typeparam name="TExtension">The type of the extension.</typeparam>
    public interface IWithBehaviorNew<TExtension> : IExecuteActionNew<TExtension>,
                                                 IExecuteActionOnExtensionNew<TExtension>,
                                                 IExecuteActionOnExtensionWithContextNew<TExtension>
        where TExtension : IExtension
    {
        /// <summary>
        /// Attaches a behavior to the currently built executable.
        /// </summary>
        /// <param name="behavior">The behavior.</param>
        /// <returns>
        /// The syntax.
        /// </returns>
        IWithBehaviorNew<TExtension> Apply(IBehavior<TExtension> behavior);

        /// <summary>
        /// Attaches a lazy behavior to the currently built executable.
        /// </summary>
        /// <param name="behavior">The behavior.</param>
        /// <returns>
        /// The syntax.
        /// </returns>
        IWithBehaviorNew<TExtension> Apply(Expression<Func<IBehavior<TExtension>>> behavior);
    }

    /// <summary>
    /// Fluent definition syntax interface for behaviors which operate on contexts.
    /// </summary>
    /// <typeparam name="TExtension">The type of the extension.</typeparam>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    public interface IWithBehaviorOnContextNew<TExtension, TContext> : IWithBehaviorNew<TExtension>
        where TExtension : IExtension
    {
        /// <summary>
        /// Attaches a behavior which has access to the context to the currently built executable.
        /// </summary>
        /// <param name="provider">The behavior provider.</param>
        /// <returns>The syntax.</returns>
        IWithBehaviorOnContextNew<TExtension, TContext> Apply(Expression<Func<TContext, IBehavior<TExtension>>> provider);
    }
}
//-------------------------------------------------------------------------------
// <copyright file="AbstractEventScopeContext.cs" company="Appccelerate">
//   Copyright (c) 2008-2012
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
//-------------------------------------------------------------------------------

namespace Appccelerate.ScopingEventBroker
{
    using System;

    /// <summary>
    /// Abstract scope which handles the creation of the current scope.
    /// </summary>
    public abstract class AbstractEventScopeContext : IEventScopeContext, IEventScopeHolder
    {
        private readonly Func<IEventScopeInternal> scopeFactory;

        protected AbstractEventScopeContext(Func<IEventScopeInternal> eventScopeFactory)
        {
            this.scopeFactory = eventScopeFactory;
        }

        public IEventScopeRegistry Current
        {
            get { return this.CurrentScope; }
        }

        protected abstract ScopeDecorator CurrentScope { get; set; }

        public IEventScope Acquire()
        {
            return this.CurrentScope ?? (this.CurrentScope = new ScopeDecorator(this.scopeFactory(), this.ResetAction));
        }

        protected virtual void ResetAction()
        {
            this.CurrentScope = null;
        }

        /// <summary>
        /// Scope decorator which allows to dynamically release the <see cref="CurrentScope"/> 
        /// </summary>
        protected class ScopeDecorator : IEventScopeInternal
        {
            private readonly IEventScopeInternal scope;
            private readonly Action action;

            public ScopeDecorator(IEventScopeInternal scope, Action action)
            {
                this.action = action;
                this.scope = scope;
            }

            public void Release()
            {
                this.scope.Release();
            }

            public void Cancel()
            {
                this.scope.Cancel();
            }

            public void Register(Action releaseCallback)
            {
                this.scope.Register(releaseCallback);
            }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (disposing)
                {
                    this.scope.Dispose();
                    this.action();
                }
            }
        }
    }
}
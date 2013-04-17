using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace SourceControlForOracle.Observables
{
    public class ComputedValue<T> : INotifyPropertyChanged, IDisposable
    {
        private readonly Dictionary<PropertyChangedEventHandler, WeakReference> m_Handlers
            = new Dictionary<PropertyChangedEventHandler, WeakReference>();

        private readonly Func<T> m_ValueFunc;

        public T Value
        {
            get
            {
                T result = m_ValueFunc();
                return result;
            }
        }

        public static implicit operator T(ComputedValue<T> computedValue)
        {
            return computedValue.Value;
        }

        public ComputedValue(Expression<Func<T>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            Expression body = expression.Body;

            ProcessDependents(body);

            m_ValueFunc = expression.Compile();
        }

        private void ProcessDependents(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    ProcessUnaryExpression((UnaryExpression) expression);
                    break;
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    ProcessBinaryExpression((BinaryExpression) expression);
                    break;
//				case ExpressionType.TypeIs:
//					return this.ProcessTypeIs((TypeBinaryExpression)expression);
//				case ExpressionType.Conditional:
//					return this.ProcessConditional((ConditionalExpression)expression);
//				case ExpressionType.Constant:
//					return this.ProcessConstant((ConstantExpression)expression);
//				case ExpressionType.Parameter:
//					return this.ProcessParameter((ParameterExpression)expression);
                case ExpressionType.MemberAccess:
                    ProcessMemberAccessExpression((MemberExpression) expression);
                    break;
                case ExpressionType.Call:
                    ProcessMethodCallExpression((MethodCallExpression) expression);
                    break;
//				case ExpressionType.Lambda:
//					return this.ProcessLambda((LambdaExpression)expression);
//				case ExpressionType.New:
//					return this.ProcessNew((NewExpression)expression);
//				case ExpressionType.NewArrayInit:
//				case ExpressionType.NewArrayBounds:
//					return this.ProcessNewArray((NewArrayExpression)expression);
//				case ExpressionType.Invoke:
//					return this.ProcessInvocation((InvocationExpression)expression);
//				case ExpressionType.MemberInit:
//					return this.ProcessMemberInit((MemberInitExpression)expression);
//				case ExpressionType.ListInit:
//					return this.ProcessListInit((ListInitExpression)expression);
                default:
                    return;
            }
        }

        private void ProcessMethodCallExpression(MethodCallExpression expression)
        {
            foreach (Expression argumentExpression in expression.Arguments)
            {
                ProcessDependents(argumentExpression);
            }
        }

        private void ProcessUnaryExpression(UnaryExpression expression)
        {
            ProcessDependents(expression.Operand);
        }

        private void ProcessBinaryExpression(BinaryExpression binaryExpression)
        {
            Expression left = binaryExpression.Left;
            Expression right = binaryExpression.Right;
            ProcessDependents(left);
            ProcessDependents(right);
        }

        private void ProcessMemberAccessExpression(MemberExpression expression)
        {
            Expression ownerExpression = expression.Expression;
            Type ownerExpressionType = ownerExpression.Type;

            if (typeof(INotifyPropertyChanged).IsAssignableFrom(ownerExpressionType))
            {
                try
                {
                    string memberName = expression.Member.Name;
                    PropertyChangedEventHandler handler = delegate(object sender, PropertyChangedEventArgs args)
                    {
                        if (args.PropertyName == memberName)
                        {
                            OnValueChanged();
                        }
                    };

                    var owner = (INotifyPropertyChanged) GetValue(ownerExpression);
                    owner.PropertyChanged += handler;

                    m_Handlers[handler] = new WeakReference(owner);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ComputedValue failed to resolve INotifyPropertyChanged value for property {0} {1}",
                                      expression.Member, ex);
                }
            }
        }

        private object GetValue(Expression expression)
        {
            UnaryExpression unaryExpression = Expression.Convert(expression, typeof(object));
            Expression<Func<object>> getterLambda = Expression.Lambda<Func<object>>(unaryExpression);
            Func<object> getter = getterLambda.Compile();

            return getter();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnValueChanged()
        {
            OnPropertyChanged("Value");
        }

        private bool disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    foreach (KeyValuePair<PropertyChangedEventHandler, WeakReference> pair in m_Handlers)
                    {
                        INotifyPropertyChanged target = pair.Value.Target as INotifyPropertyChanged;
                        if (target != null)
                        {
                            target.PropertyChanged -= pair.Key;
                        }
                    }

                    m_Handlers.Clear();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
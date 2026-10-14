using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SystemInfoCollector.ViewModels
{
    /// <summary>
    /// Base para ViewModels, com notificação de alteração de propriedades.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Disparado quando o valor de uma propriedade muda.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifica a alteração da propriedade informada.
        /// </summary>
        /// <param name="propertyName">Nome da propriedade; preenchido automaticamente pelo compilador.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Atribui o valor ao campo e notifica a alteração, somente se o valor mudou.
        /// </summary>
        /// <returns><c>true</c> se o valor foi alterado.</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}

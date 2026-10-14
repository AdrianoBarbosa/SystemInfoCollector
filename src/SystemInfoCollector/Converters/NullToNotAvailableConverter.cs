using System;
using System.Globalization;
using System.Windows.Data;

namespace SystemInfoCollector.Converters
{
    /// <summary>
    /// Converte valores nulos ou vazios no texto "Não disponível" para exibição.
    /// </summary>
    public class NullToNotAvailableConverter : IValueConverter
    {
        /// <summary>
        /// Texto exibido quando uma informação não pôde ser obtida.
        /// </summary>
        public const string NotAvailableText = "Não disponível";

        /// <summary>
        /// Retorna <see cref="NotAvailableText"/> para valores nulos ou vazios; caso contrário, o próprio valor.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || (value is string text && string.IsNullOrWhiteSpace(text)))
            {
                return NotAvailableText;
            }

            return value;
        }

        /// <summary>
        /// Não suportado: a conversão é apenas para exibição.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

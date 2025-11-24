// Aluno: Vinicius de Liz da Conceição
using AcademiaDoZe.Application.Enums;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace AcademiaDoZe.Presentation.AppMaui.Converters
{
    /// <summary>
    /// Converte um enum de flags em uma lista de nomes de display formatados
    /// </summary>
    public class RestricoesMedicasListConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not EAppMatriculaRestricoes restricoes || restricoes == EAppMatriculaRestricoes.None)
                return new List<string>();

            var lista = new List<string>();

            // Percorre todos os valores do enum que são flags ativas
            foreach (EAppMatriculaRestricoes flag in Enum.GetValues(typeof(EAppMatriculaRestricoes)))
            {
                // Ignora None e verifica se a flag está ativa
                if (flag != EAppMatriculaRestricoes.None && restricoes.HasFlag(flag))
                {
                    // Obtém o nome do Display attribute
                    var displayName = GetDisplayName(flag);
                    lista.Add($"• {displayName}");
                }
            }

            return lista;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static string GetDisplayName(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttribute<DisplayAttribute>();
            return attribute?.Name ?? value.ToString();
        }
    }
}
using AcademiaDoZe.Application.DTOs;
using System.Globalization;

namespace AcademiaDoZe.Presentation.AppMaui.Converters
{
    public class FotoToImageSourceConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ArquivoDTO arquivo && arquivo.Conteudo != null && arquivo.Conteudo.Length > 0)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] FotoConverter - Tamanho: {arquivo.Conteudo.Length} bytes");
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] FotoConverter - ContentType: {arquivo.ContentType}");
                    
                    var imageSource = ImageSource.FromStream(() => new MemoryStream(arquivo.Conteudo));
                    
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] ✅ FotoConverter - ImageSource criado!");
                    return imageSource;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] ❌ FotoConverter - {ex.Message}");
                    return null;
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"[WARN] FotoConverter - Sem conteúdo");
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
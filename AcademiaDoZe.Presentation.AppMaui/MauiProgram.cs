﻿using AcademiaDoZe.Presentation.AppMaui.ViewModels;
using AcademiaDoZe.Presentation.AppMaui.Views;
using Microsoft.Extensions.Logging;
using AcademiaDoZe.Presentation.AppMaui.Configuration;
namespace AcademiaDoZe.Presentation.AppMaui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                // Adicione esta linha para registrar a fonte
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
            });
            // Configurar serviços da aplicação e repositórios
            ConfigurationHelper.ConfigureServices(builder.Services);
            // Registrar ViewModels

            builder.Services.AddTransient<DashboardListViewModel>();
            builder.Services.AddTransient<LogradouroListViewModel>();
            builder.Services.AddTransient<LogradouroViewModel>();
            builder.Services.AddTransient<ColaboradorListViewModel>();
            builder.Services.AddTransient<ColaboradorViewModel>();
            builder.Services.AddTransient<AlunoListViewModel>();
            builder.Services.AddTransient<AlunoViewModel>();
            // Registrar Views
            builder.Services.AddTransient<DashboardListPage>();
            builder.Services.AddTransient<LogradouroListPage>();
            builder.Services.AddTransient<LogradouroPage>();
            builder.Services.AddTransient<ConfigPage>();
            builder.Services.AddTransient<ColaboradorListPage>();
            builder.Services.AddTransient<ColaboradorPage>();
            builder.Services.AddTransient<AlunoListPage>();
            builder.Services.AddTransient<AlunoPage>();
#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}

// Terminei a atividade do Lúcio, preciso arrumar o erro de entrada sobre arquivo invalido e porque outras contagens sumiram
// Também decorar as corzinha no meu programa e arrumar os testes
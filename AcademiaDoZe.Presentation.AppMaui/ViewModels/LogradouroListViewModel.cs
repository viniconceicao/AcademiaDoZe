using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels
{
    public partial class LogradouroListViewModel : BaseViewModel
    {
        public ObservableCollection<string> FilterTypes { get; } = new() { "Cidade", "Id", "Cep" };
        private readonly ILogradouroService _logradouroService;
        private string _searchText = string.Empty;
        public string SearchText { get => _searchText; set => SetProperty(ref _searchText, value); }
        
        private string _selectedFilterType = "Cidade";
        public string SelectedFilterType
        {
            get => _selectedFilterType; 
            set => SetProperty(ref _selectedFilterType, value);
        }
        
        private ObservableCollection<LogradouroDTO> _logradouros = new();
        public ObservableCollection<LogradouroDTO> Logradouros 
        { 
            get => _logradouros; 
            set => SetProperty(ref _logradouros, value); 
        }
        
        private LogradouroDTO? _selectedLogradouro;
        public LogradouroDTO? SelectedLogradouro 
        { 
            get => _selectedLogradouro; 
            set => SetProperty(ref _selectedLogradouro, value); 
        }

        public LogradouroListViewModel(ILogradouroService logradouroService)
        {
            _logradouroService = logradouroService ?? throw new ArgumentNullException(nameof(logradouroService));
            Title = "Logradouros";
            Logradouros = new ObservableCollection<LogradouroDTO>();
            
            System.Diagnostics.Debug.WriteLine("[DEBUG] LogradouroListViewModel - Construtor executado");
        }

        [RelayCommand]
        private async Task AddLogradouroAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] AddLogradouro - Navegando para nova tela");
                await Shell.Current.GoToAsync("logradouro");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] AddLogradouro - {ex.Message}");
                await Shell.Current.DisplayAlert("Erro", $"Erro ao navegar: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task EditLogradouroAsync(LogradouroDTO logradouro)
        {
            try
            {
                if (logradouro == null)
                    return;
                    
                System.Diagnostics.Debug.WriteLine($"[DEBUG] EditLogradouro - ID: {logradouro.Id}");
                await Shell.Current.GoToAsync($"logradouro?Id={logradouro.Id}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] EditLogradouro - {ex.Message}");
                await Shell.Current.DisplayAlert("Erro", $"Erro ao navegar: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            IsRefreshing = true;
            await LoadLogradourosAsync();
        }

        [RelayCommand]
        private async Task SearchLogradourosAsync()
        {
            if (IsBusy)
                return;
                
            try
            {
                IsBusy = true;
                System.Diagnostics.Debug.WriteLine($"[DEBUG] SearchLogradouros - Filtro: {SelectedFilterType}, Texto: {SearchText}");
                
                await MainThread.InvokeOnMainThreadAsync(() => Logradouros.Clear());
                
                IEnumerable<LogradouroDTO> resultados = Enumerable.Empty<LogradouroDTO>();
                
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    resultados = await _logradouroService.ObterTodosAsync() ?? Enumerable.Empty<LogradouroDTO>();
                }
                else if (SelectedFilterType == "Cidade")
                {
                    resultados = await _logradouroService.ObterPorCidadeAsync(SearchText) ?? Enumerable.Empty<LogradouroDTO>();
                }
                else if (SelectedFilterType == "Id" && int.TryParse(SearchText, out int id))
                {
                    var logradouro = await _logradouroService.ObterPorIdAsync(id);
                    if (logradouro != null)
                        resultados = new[] { logradouro };
                }
                else if (SelectedFilterType == "Cep")
                {
                    var logradouro = await _logradouroService.ObterPorCepAsync(SearchText);
                    if (logradouro != null)
                        resultados = new[] { logradouro };
                }
                
                System.Diagnostics.Debug.WriteLine($"[DEBUG] SearchLogradouros - Encontrados: {resultados.Count()}");
                
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var item in resultados)
                    {
                        Logradouros.Add(item);
                    }
                    OnPropertyChanged(nameof(Logradouros));
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] SearchLogradouros - {ex.Message}");
                await Shell.Current.DisplayAlert("Erro", $"Erro ao buscar: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task LoadLogradourosAsync()
        {
            if (IsBusy)
                return;
                
            try
            {
                IsBusy = true;
                System.Diagnostics.Debug.WriteLine("[DEBUG] LoadLogradouros - Iniciando");

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Logradouros.Clear();
                    OnPropertyChanged(nameof(Logradouros));
                });

                var logradourosList = await _logradouroService.ObterTodosAsync();
                System.Diagnostics.Debug.WriteLine($"[DEBUG] LoadLogradouros - Retornados: {logradourosList?.Count() ?? 0}");

                if (logradourosList != null && logradourosList.Any())
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        foreach (var logradouro in logradourosList)
                        {
                            System.Diagnostics.Debug.WriteLine($"[DEBUG] Adicionando - ID: {logradouro.Id}, Nome: {logradouro.Nome}");
                            Logradouros.Add(logradouro);
                        }
                        OnPropertyChanged(nameof(Logradouros));
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] Total na coleção: {Logradouros.Count}");
                    });
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[WARN] Nenhum logradouro retornado");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] LoadLogradouros - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                await Shell.Current.DisplayAlert("Erro", $"Erro ao carregar: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private async Task DeleteLogradouroAsync(LogradouroDTO logradouro)
        {
            if (logradouro == null)
                return;
                
            bool confirm = await Shell.Current.DisplayAlert(
                "Confirmar Exclusão",
                $"Deseja realmente excluir o logradouro {logradouro.Nome}?",
                "Sim", "Não");
                
            if (!confirm)
                return;
                
            try
            {
                IsBusy = true;
                System.Diagnostics.Debug.WriteLine($"[DEBUG] DeleteLogradouro - ID: {logradouro.Id}");
                
                bool success = await _logradouroService.RemoverAsync(logradouro.Id);
                if (success)
                {
                    Logradouros.Remove(logradouro);
                    await Shell.Current.DisplayAlert("Sucesso", "Logradouro excluído com sucesso!", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Erro", "Não foi possível excluir o logradouro.", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] DeleteLogradouro - {ex.Message}");
                await Shell.Current.DisplayAlert("Erro", $"Erro ao excluir: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
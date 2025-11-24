using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels
{
    public partial class MatriculaListViewModel : BaseViewModel
    {
        public ObservableCollection<string> FilterTypes { get; } = new() { "Id", "Aluno" };
        
        private readonly IMatriculaService _matriculaService;
        
        
        private List<MatriculaDTO> _todasMatriculas = new();
        
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }
        
        private string _selectedFilterType = "Aluno";
        public string SelectedFilterType
        {
            get => _selectedFilterType;
            set => SetProperty(ref _selectedFilterType, value);
        }   
        
        private ObservableCollection<MatriculaDTO> _matriculas = new();
        public ObservableCollection<MatriculaDTO> Matriculas
        {
            get => _matriculas;
            set => SetProperty(ref _matriculas, value);
        }

        public MatriculaListViewModel(IMatriculaService matriculaService)
        {
            _matriculaService = matriculaService;
            Title = "Matriculas";
            System.Diagnostics.Debug.WriteLine("[DEBUG] MatriculaListViewModel - Construtor executado");
        }

        [RelayCommand]
        private async Task AddMatriculaAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("matricula");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao navegar: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task EditMatriculaAsync(MatriculaDTO matricula)
        {
            try
            {
                if (matricula == null)
                    return;
                await Shell.Current.GoToAsync($"matricula?Id={matricula.Id}");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao navegar: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            IsRefreshing = true;
            await LoadMatriculasAsync();
        }

        
        [RelayCommand]
        private async Task FilterMatriculasAsync()
        {
            if (IsBusy)
                return;
                
            try
            {
                IsBusy = true;
                
               
                var searchTextTrimmed = SearchText?.Trim() ?? string.Empty;
                
                System.Diagnostics.Debug.WriteLine($"[DEBUG] FilterMatriculas - Tipo: {SelectedFilterType}, Texto original: '{SearchText}', Texto limpo: '{searchTextTrimmed}'");
                
                
                if (string.IsNullOrWhiteSpace(searchTextTrimmed))
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] FilterMatriculas - Campo vazio, recarregando tudo");
                    IsBusy = false; 
                    await LoadMatriculasAsync();
                    return;
                }
                
                await MainThread.InvokeOnMainThreadAsync(() => Matriculas.Clear());
                
                // Se a lista de backup está vazia, carrega primeiro
                if (!_todasMatriculas.Any())
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] FilterMatriculas - Carregando lista de backup");
                    var todas = await _matriculaService.ObterTodasAsync();
                    _todasMatriculas = todas?.ToList() ?? new List<MatriculaDTO>();
                }
                
                System.Diagnostics.Debug.WriteLine($"[DEBUG] FilterMatriculas - Total no backup: {_todasMatriculas.Count}");
                
                // Filtra conforme o tipo selecionado
                IEnumerable<MatriculaDTO> resultados = Enumerable.Empty<MatriculaDTO>();
                
                if (SelectedFilterType == "Id")
                {
                    if (int.TryParse(searchTextTrimmed, out int id))
                    {
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] FilterMatriculas - Buscando por ID: {id}");
                        resultados = _todasMatriculas.Where(m => m.Id == id);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] FilterMatriculas - ID inválido: '{searchTextTrimmed}'");
                        await Shell.Current.DisplayAlert("Aviso", "Digite um número válido para buscar por ID.", "OK");
                        IsBusy = false; 
                        await LoadMatriculasAsync();
                        return;
                    }
                }
                else if (SelectedFilterType == "Aluno")
                {
                    var searchLower = searchTextTrimmed.ToLower();
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] FilterMatriculas - Buscando por nome: '{searchLower}'");
                    
                    resultados = _todasMatriculas.Where(m => 
                        m.AlunoMatricula != null && 
                        !string.IsNullOrEmpty(m.AlunoMatricula.Nome) &&
                        m.AlunoMatricula.Nome.ToLower().Contains(searchLower));
                }
                
                var lista = resultados.ToList();
                System.Diagnostics.Debug.WriteLine($"[DEBUG] FilterMatriculas - Encontrados: {lista.Count}");
                
                if (!lista.Any())
                {
                    await Shell.Current.DisplayAlert("Aviso", "Nenhum resultado encontrado.", "OK");
                    IsBusy = false; 
                    await LoadMatriculasAsync();
                    return;
                }
                
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var item in lista)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] Adicionando matrícula #{item.Id} - Aluno: {item.AlunoMatricula?.Nome}");
                        Matriculas.Add(item);
                    }
                    OnPropertyChanged(nameof(Matriculas));
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] FilterMatriculas - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                await Shell.Current.DisplayAlert("Erro", $"Erro ao filtrar: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false; 
            }
        }

        [RelayCommand]
        private async Task LoadMatriculasAsync()
        {
            if (IsBusy)
                return;
                
            try
            {
                IsBusy = true;
                System.Diagnostics.Debug.WriteLine("[DEBUG] LoadMatriculas - Iniciando");

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Matriculas.Clear();
                    OnPropertyChanged(nameof(Matriculas));
                });

                var matriculasList = await _matriculaService.ObterTodasAsync();
                System.Diagnostics.Debug.WriteLine($"[DEBUG] LoadMatriculas - Retornados: {matriculasList?.Count() ?? 0}");

                
                _todasMatriculas = matriculasList?.ToList() ?? new List<MatriculaDTO>();

                if (matriculasList != null && matriculasList.Any())
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        foreach (var matricula in matriculasList)
                        {
                            Matriculas.Add(matricula);
                        }
                        OnPropertyChanged(nameof(Matriculas));
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] LoadMatriculas - {ex.Message}");
                await Shell.Current.DisplayAlert("Erro", $"Erro ao carregar matriculas: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private async Task DeleteMatriculaAsync(MatriculaDTO matricula)
        {
            if (matricula == null)
                return;
                
            bool confirm = await Shell.Current.DisplayAlert(
                "Confirmar Exclusao",
                $"Deseja realmente excluir a matricula?",
                "Sim", "Nao");
                
            if (!confirm)
                return;
                
            try
            {
                IsBusy = true;
                bool success = await _matriculaService.RemoverAsync(matricula.Id);
                if (success)
                {
                    Matriculas.Remove(matricula);
                    _todasMatriculas.Remove(matricula);
                    await Shell.Current.DisplayAlert("Sucesso", "Matricula excluida com sucesso!", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Erro", "Nao foi possivel excluir a matricula.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao excluir matricula: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
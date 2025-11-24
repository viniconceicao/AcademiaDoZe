using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels
{
    // Classe helper para gerenciar checkboxes das restrições
    public class RestricaoCheckItem : INotifyPropertyChanged
    {
        public EAppMatriculaRestricoes Restricao { get; set; }
        public string Nome { get; set; } = string.Empty;
        
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    [QueryProperty(nameof(MatriculaId), "Id")]
    public partial class MatriculaViewModel : BaseViewModel
    {
        public IEnumerable<EAppMatriculaPlano> Planos { get; } = Enum.GetValues(typeof(EAppMatriculaPlano)).Cast<EAppMatriculaPlano>();
        
        // ObservableCollection para as restrições com checkboxes
        public ObservableCollection<RestricaoCheckItem> RestricoesDisponiveis { get; set; }

        private readonly IMatriculaService _matriculaService;
        private readonly IAlunoService _alunoService;

        // Flag para controlar se os eventos devem ser processados
        private bool _isUpdatingCheckboxes = false;

        private MatriculaDTO _matricula = new()
        {
            AlunoMatricula = new AlunoDTO
            {
                Nome = string.Empty,
                Cpf = string.Empty,
                DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
                Telefone = string.Empty,
                Endereco = new LogradouroDTO 
                { 
                    Cep = string.Empty, 
                    Nome = string.Empty, 
                    Bairro = string.Empty, 
                    Cidade = string.Empty, 
                    Estado = string.Empty, 
                    Pais = string.Empty 
                },
                Numero = string.Empty
            },
            Plano = EAppMatriculaPlano.Mensal,
            DataInicio = DateOnly.FromDateTime(DateTime.Today),
            DataFim = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
            Objetivo = string.Empty,
            RestricoesMedicas = EAppMatriculaRestricoes.None
        };

        public MatriculaDTO Matricula
        {
            get => _matricula;
            set => SetProperty(ref _matricula, value);
        }

        private int _matriculaId;
        public int MatriculaId
        {
            get => _matriculaId;
            set
            {
                if (SetProperty(ref _matriculaId, value))
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] MatriculaId alterado para: {value}");
                }
            }
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public MatriculaViewModel(IMatriculaService matriculaService, IAlunoService alunoService)
        {
            _matriculaService = matriculaService;
            _alunoService = alunoService;
            Title = "Detalhes da Matrícula";
            
            // Inicializa a lista de restrições (excluindo "None")
            RestricoesDisponiveis = new ObservableCollection<RestricaoCheckItem>();
            var restricoes = Enum.GetValues(typeof(EAppMatriculaRestricoes))
                .Cast<EAppMatriculaRestricoes>()
                .Where(r => r != EAppMatriculaRestricoes.None)
                .OrderBy(r => (int)r);

            foreach (var restricao in restricoes)
            {
                var item = new RestricaoCheckItem
                {
                    Restricao = restricao,
                    Nome = restricao.GetDisplayName(),
                    IsSelected = false
                };
                
                // Registra o evento de mudança para atualizar automaticamente
                item.PropertyChanged += OnRestricaoCheckItemPropertyChanged;
                
                RestricoesDisponiveis.Add(item);
            }
        }

        // Atualiza o enum Flags com base nos checkboxes selecionados
        private void AtualizarRestricoesMatricula()
        {
            if (_isUpdatingCheckboxes)
            {
                return;
            }

            var restricoesSelecionadas = EAppMatriculaRestricoes.None;
            
            foreach (var item in RestricoesDisponiveis.Where(r => r.IsSelected))
            {
                restricoesSelecionadas |= item.Restricao;
            }
            
            Matricula.RestricoesMedicas = restricoesSelecionadas;
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Restrições atualizadas: {Matricula.RestricoesMedicas} (valor: {(int)Matricula.RestricoesMedicas})");
        }

        // Atualiza os checkboxes com base no enum Flags
        private void AtualizarCheckboxesRestrices(EAppMatriculaRestricoes restricoes)
        {
            _isUpdatingCheckboxes = true;
            
            try
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] AtualizarCheckboxesRestrices - Restrições: {restricoes} (valor: {(int)restricoes})");
                
                foreach (var item in RestricoesDisponiveis)
                {
                    item.IsSelected = restricoes.HasFlag(item.Restricao);
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] AtualizarCheckboxesRestrices - {item.Nome}: {item.IsSelected}");
                }
            }
            finally
            {
                _isUpdatingCheckboxes = false;
            }
        }

        private void OnRestricaoCheckItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RestricaoCheckItem.IsSelected))
            {
                AtualizarRestricoesMatricula();
            }
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        public async Task InitializeAsync()
        {
            if (MatriculaId > 0)
            {
                IsEditMode = true;
                Title = "Editar Matrícula";
                await LoadMatriculaAsync();
            }
            else
            {
                IsEditMode = false;
                Title = "Nova Matrícula";
            }
        }

        [RelayCommand]
        public async Task LoadMatriculaAsync()
        {
            if (MatriculaId <= 0) return;

            try
            {
                IsBusy = true;
                System.Diagnostics.Debug.WriteLine($"[DEBUG] LoadMatricula - MatriculaId: {MatriculaId}");
                
                var matriculaData = await _matriculaService.ObterPorIdAsync(MatriculaId);

                if (matriculaData != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] LoadMatricula - Matrícula carregada: ID={matriculaData.Id}");
                    
                    Matricula = matriculaData;
                    
                    AtualizarCheckboxesRestrices(matriculaData.RestricoesMedicas);
                    
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] LoadMatricula - Matricula.Id após load: {Matricula.Id}");
                    
                    OnPropertyChanged(nameof(Matricula));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] LoadMatricula - {ex.Message}");
                await Shell.Current.DisplayAlert("Erro", $"Erro ao carregar matrícula: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task SaveMatriculaAsync()
        {
            if (IsBusy) return;
            if (!ValidateMatricula(Matricula)) return;

            try
            {
                IsBusy = true;

                System.Diagnostics.Debug.WriteLine($"[DEBUG] SaveMatricula - Início");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] SaveMatricula - IsEditMode: {IsEditMode}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] SaveMatricula - MatriculaId (propriedade): {MatriculaId}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] SaveMatricula - Matricula.Id: {Matricula.Id}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] SaveMatricula - Aluno ID: {Matricula.AlunoMatricula.Id}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] SaveMatricula - Aluno Nome: {Matricula.AlunoMatricula.Nome}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] SaveMatricula - Restrições: {Matricula.RestricoesMedicas}");

                if (IsEditMode)
                {
                    if (Matricula.Id == 0 && MatriculaId > 0)
                    {
                        Matricula.Id = MatriculaId;
                        System.Diagnostics.Debug.WriteLine($"[WARNING] SaveMatricula - ID da matrícula estava zerado, corrigido para: {Matricula.Id}");
                    }
                    
                    if (Matricula.Id <= 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ERROR] SaveMatricula - ID inválido! Matricula.Id={Matricula.Id}, MatriculaId={MatriculaId}");
                        await Shell.Current.DisplayAlert("Erro", $"ID da matrícula inválido: {Matricula.Id}. Não é possível atualizar.", "OK");
                        return;
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] SaveMatricula - Chamando AtualizarAsync com ID: {Matricula.Id}");
                    
                    var atualizada = await _matriculaService.AtualizarAsync(Matricula);
                    if (atualizada != null)
                    {
                        await Shell.Current.DisplayAlert("Sucesso", "Matrícula atualizada com sucesso!", "OK");
                        await Shell.Current.GoToAsync("..");
                    }
                }
                else
                {
                    var criada = await _matriculaService.AdicionarAsync(Matricula);
                    if (criada != null)
                    {
                        await Shell.Current.DisplayAlert("Sucesso", "Matrícula criada com sucesso!", "OK");
                        await Shell.Current.GoToAsync("..");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] SaveMatricula - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ERROR] SaveMatricula - InnerException: {ex.InnerException?.Message}");
                await Shell.Current.DisplayAlert("Erro", $"Erro ao salvar matrícula: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task SearchAlunoByCpfAsync()
        {
            if (string.IsNullOrWhiteSpace(Matricula.AlunoMatricula.Cpf)) 
            {
                await Shell.Current.DisplayAlert("Aviso", "Digite o CPF do aluno.", "OK");
                return;
            }

            try
            {
                IsBusy = true;
                var cpfNormalized = new string(Matricula.AlunoMatricula.Cpf.Where(char.IsDigit).ToArray());
                
                System.Diagnostics.Debug.WriteLine($"[DEBUG] SearchAlunoByCpf - CPF normalizado: {cpfNormalized}");
                
                var aluno = await _alunoService.ObterPorCpfAsync(cpfNormalized);

                if (aluno != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] SearchAlunoByCpf - Aluno encontrado: ID={aluno.Id}, Nome={aluno.Nome}");
                    
                    Matricula.AlunoMatricula = aluno;
                    OnPropertyChanged(nameof(Matricula));
                    
                    await Shell.Current.DisplayAlert("Sucesso", $"Aluno encontrado: {aluno.Nome}", "OK");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] SearchAlunoByCpf - Aluno NÃO encontrado");
                    await Shell.Current.DisplayAlert("Aviso", "Aluno não encontrado. Verifique o CPF digitado.", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] SearchAlunoByCpf - {ex.Message}");
                await Shell.Current.DisplayAlert("Erro", $"Erro ao buscar aluno: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task SelecionarLaudoAsync()
        {
            try
            {
                string escolha = await Shell.Current.DisplayActionSheet(
                    "Selecionar Laudo Médico", 
                    "Cancelar", 
                    null, 
                    "Galeria", 
                    "Câmera"
                );

                FileResult? result = null;

                if (escolha == "Galeria")
                {
                    result = await FilePicker.Default.PickAsync(new PickOptions
                    {
                        PickerTitle = "Selecione uma imagem do laudo médico",
                        FileTypes = FilePickerFileType.Images
                    });
                }
                else if (escolha == "Câmera")
                {
                    if (MediaPicker.Default.IsCaptureSupported)
                    {
                        result = await MediaPicker.Default.CapturePhotoAsync();
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Erro", "Captura de foto não suportada neste dispositivo.", "OK");
                        return;
                    }
                }

                if (result != null)
                {
                    using var stream = await result.OpenReadAsync();
                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);

                    var extensao = Path.GetExtension(result.FileName)?.ToLowerInvariant();
                    if (string.IsNullOrEmpty(extensao) || extensao == "")
                        extensao = ".jpg";

                    Matricula.LaudoMedico = new ArquivoDTO 
                    { 
                        Conteudo = ms.ToArray(),
                        ContentType = extensao
                    };

                   
                    if (Matricula.LaudoMedico?.Conteudo != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Laudo SALVO na memória! Tamanho: {Matricula.LaudoMedico.Conteudo.Length} bytes");
                    }

                 
                    OnPropertyChanged(nameof(Matricula));
                    OnPropertyChanged(nameof(Matricula.LaudoMedico));
                    
                    await Shell.Current.DisplayAlert(
                        "Sucesso", 
                        $"Laudo selecionado: {result.FileName}", 
                        "OK"
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] ❌ SelecionarLaudo - {ex.Message}");
                await Shell.Current.DisplayAlert("Erro", $"Erro ao selecionar laudo: {ex.Message}", "OK");
            }
        }

        private static bool ValidateMatricula(MatriculaDTO matricula)
        {
            const string validationTitle = "Validação";

            if (matricula.AlunoMatricula == null || matricula.AlunoMatricula.Id <= 0)
            {
                Shell.Current.DisplayAlert(validationTitle, "Aluno é obrigatório. Use a busca por CPF para selecionar um aluno cadastrado.", "OK");
                return false;
            }

            if (matricula.DataInicio == default)
            {
                Shell.Current.DisplayAlert(validationTitle, "Data de início é obrigatória.", "OK");
                return false;
            }

            if (matricula.DataFim == default)
            {
                Shell.Current.DisplayAlert(validationTitle, "Data de fim é obrigatória.", "OK");
                return false;
            }

            if (matricula.DataFim <= matricula.DataInicio)
            {
                Shell.Current.DisplayAlert(validationTitle, "Data de fim deve ser posterior à data de início.", "OK");
                return false;
            }

            if (string.IsNullOrWhiteSpace(matricula.Objetivo))
            {
                Shell.Current.DisplayAlert(validationTitle, "Objetivo é obrigatório.", "OK");
                return false;
            }

            if (matricula.RestricoesMedicas != EAppMatriculaRestricoes.None && matricula.LaudoMedico == null)
            {
                Shell.Current.DisplayAlert(validationTitle, "Laudo médico é obrigatório quando há restrições médicas.", "OK");
                return false;
            }

            return true;
        }
    }
}
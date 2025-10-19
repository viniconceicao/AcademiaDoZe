using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Services;
using AcademiaDoZe.Domain.Entities;
using CommunityToolkit.Mvvm.Input;
namespace AcademiaDoZe.Presentation.AppMaui.ViewModels
{
    [QueryProperty(nameof(AlunoId), "Id")]
    public partial class AlunoViewModel : BaseViewModel
    {
        private readonly IAlunoService _alunoService;
        private readonly ILogradouroService _logradouroService;
        private AlunoDTO _aluno = new()
        {
            Nome = string.Empty,
            Cpf = string.Empty,
            DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
            Telefone = string.Empty,
            Email = string.Empty,
            Endereco = new LogradouroDTO { Cep = string.Empty, Nome = string.Empty, Bairro = string.Empty, Cidade = string.Empty, Estado = string.Empty, Pais = string.Empty },
            Numero = string.Empty,
        };
        public AlunoDTO Aluno
        {
            get => _aluno;
            set => SetProperty(ref _aluno, value);
        }

        private int _alunoId;
        public int AlunoId
        {
            get => _alunoId;
            set => SetProperty(ref _alunoId, value);
        }
        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }
        public AlunoViewModel(IAlunoService alunoService, ILogradouroService logradouroService)
        {
            _alunoService = alunoService;
            _logradouroService = logradouroService;
            Title = "Detalhes do Aluno";
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
        public async Task InitializeAsync()
        {
            if (AlunoId > 0)
            {
                IsEditMode = true;
                Title = "Editar Aluno";
                await LoadAlunoAsync();
            }
            else
            {
                IsEditMode = false;
                Title = "Novo Aluno";
            }
        }
        [RelayCommand]
        public async Task LoadAlunoAsync()
        {
            if (AlunoId <= 0)
                return;
            try
            {
                IsBusy = true;
                var alunoData = await _alunoService.ObterPorIdAsync(AlunoId);

                if (alunoData != null)

                {
                    Aluno = alunoData;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao carregar aluno: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task SaveAlunoAsync()
        {
            if (IsBusy)
                return;
            if (!ValidateAluno(Aluno))
                return;
            try
            {
                IsBusy = true;
                // Verifica se o CEP existe antes de continuar

                var logradouroData = await _logradouroService.ObterPorCepAsync(Aluno.Endereco.Cep);
                if (logradouroData == null)

                {
                    await Shell.Current.DisplayAlert("Erro", "O CEP informado n�o existe. O cadastro n�o pode continuar.", "OK");
                    return;
                }
                Aluno.Endereco = logradouroData;
                if (IsEditMode)
                {
                    await _alunoService.AtualizarAsync(Aluno);

                    await Shell.Current.DisplayAlert("Sucesso", "Aluno atualizado com sucesso!", "OK");

                }
                else
                {
                    await _alunoService.AdicionarAsync(Aluno);

                    await Shell.Current.DisplayAlert("Sucesso", "Aluno criado com sucesso!", "OK");

                }
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao salvar aluno: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task SearchByCpfAsync()
        {
            if (string.IsNullOrWhiteSpace(Aluno.Cpf))
                return;
            try
            {
                IsBusy = true;
                var alunoData = await _alunoService.ObterPorCpfAsync(Aluno.Cpf);

                if (alunoData != null)

                {
                    Aluno = alunoData;
                    IsEditMode = true;
                    await Shell.Current.DisplayAlert("Aviso", "Aluno j� cadastrado! Dados carregados para edi��o.", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Aviso", "CPF n�o encontrado.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao buscar CPF: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task SearchByCepAsync()
        {
            if (string.IsNullOrWhiteSpace(Aluno.Endereco.Cep))
                return;
            try
            {
                IsBusy = true;
                var logradouroData = await _logradouroService.ObterPorCepAsync(Aluno.Endereco.Cep);

                if (logradouroData != null)

                {
                    Aluno.Endereco = logradouroData;
                    OnPropertyChanged(nameof(Aluno));
                    await Shell.Current.DisplayAlert("Aviso", "CEP encontrado! Endere�o preenchido automaticamente.", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Aviso", "CEP n�o encontrado.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao buscar CEP: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task SelecionarFotoAsync()
        {
            try
            {
                string escolha = await Shell.Current.DisplayActionSheet("Origem da Imagem", "Cancelar", null, "Galeria", "C�mera");
                FileResult? result = null;
                if (escolha == "Galeria")

                {
                    result = await FilePicker.Default.PickAsync(new PickOptions
                    {
                        PickerTitle = "Selecione uma imagem",
                        FileTypes = FilePickerFileType.Images
                    });
                }
                else if (escolha == "C�mera")
                {
                    if (MediaPicker.Default.IsCaptureSupported)
                    {
                        result = await MediaPicker.Default.CapturePhotoAsync();
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Erro", "Captura de foto n�o suportada neste dispositivo.", "OK");
                        return;
                    }
                }
                if (result != null)
                {
                    using var stream = await result.OpenReadAsync();
                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    Aluno.Foto = new ArquivoDTO 
                    { 
                        Conteudo = ms.ToArray(),
                        ContentType = ".jpg" // Adiciona o ContentType
                    };
                    OnPropertyChanged(nameof(Aluno));
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao selecionar imagem: {ex.Message}", "OK");
            }
        }

        private static bool ValidateAluno(AlunoDTO aluno)
        {
            const string validationTitle = "Valida��o";
            if (string.IsNullOrWhiteSpace(aluno.Nome))
            {
                Shell.Current.DisplayAlert(validationTitle, "Nome � obrigat�rio.", "OK");
                return false;
            }
            if (string.IsNullOrWhiteSpace(aluno.Cpf) || aluno.Cpf.Length != 11)
            {
                Shell.Current.DisplayAlert(validationTitle, "CPF deve ter 11 d�gitos.", "OK");
                return false;
            }
            if (aluno.DataNascimento == default)
            {
                Shell.Current.DisplayAlert(validationTitle, "Data de nascimento � obrigat�ria.", "OK");
                return false;
            }
            if (string.IsNullOrWhiteSpace(aluno.Telefone) || aluno.Telefone.Length != 11)
            {
                Shell.Current.DisplayAlert(validationTitle, "Telefone deve ter 11 d�gitos.", "OK");
                return false;
            }
            if (aluno.Endereco == null)
            {
                Shell.Current.DisplayAlert(validationTitle, "Endere�o � obrigat�rio.", "OK");
                return false;
            }
            return true;
        }
    }
}
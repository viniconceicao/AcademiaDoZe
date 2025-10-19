﻿using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
namespace AcademiaDoZe.Presentation.AppMaui.ViewModels
{
    [QueryProperty(nameof(ColaboradorId), "Id")]
    public partial class ColaboradorViewModel : BaseViewModel
    {
        public IEnumerable<EAppColaboradorTipo> ColaboradorTipos { get; } = Enum.GetValues(typeof(EAppColaboradorTipo)).Cast<EAppColaboradorTipo>();
        public IEnumerable<EAppColaboradorVinculo> ColaboradorVinculos { get; } = Enum.GetValues(typeof(EAppColaboradorVinculo)).Cast<EAppColaboradorVinculo>();
        private readonly IColaboradorService _colaboradorService;
        private readonly ILogradouroService _logradouroService;
        private ColaboradorDTO _colaborador = new()
        {
            Nome = string.Empty,
            Cpf = string.Empty,
            DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
            Telefone = string.Empty,
            Endereco = new LogradouroDTO { Cep = string.Empty, Nome = string.Empty, Bairro = string.Empty, Cidade = string.Empty, Estado = string.Empty, Pais = string.Empty },
            Numero = string.Empty,
            DataAdmissao = DateOnly.FromDateTime(DateTime.Today),
            Tipo = EAppColaboradorTipo.Administrador,
            Vinculo = EAppColaboradorVinculo.CLT
        };
        public ColaboradorDTO Colaborador
        {
            get => _colaborador;
            set => SetProperty(ref _colaborador, value);
        }
        private int _colaboradorId;
        public int ColaboradorId
        {
            get => _colaboradorId;
            set
            {
                if (SetProperty(ref _colaboradorId, value))
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] ColaboradorId alterado para: {value}");
                    if (Colaborador != null)
                    {
                        Colaborador.Id = value;
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] Colaborador.Id atualizado para: {Colaborador.Id}");
                    }
                }
            }
        }
        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }
        public ColaboradorViewModel(IColaboradorService colaboradorService, ILogradouroService logradouroService)
        {
            _colaboradorService = colaboradorService;
            _logradouroService = logradouroService;
            Title = "Detalhes do Colaborador";
        }
        [RelayCommand]
        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
        public async Task InitializeAsync()
        {
            if (ColaboradorId > 0)
            {
                IsEditMode = true;
                Title = "Editar Colaborador";
                await LoadColaboradorAsync();
            }
            else
            {
                IsEditMode = false;
                Title = "Novo Colaborador";
            }
        }
        [RelayCommand]
        public async Task LoadColaboradorAsync()
        {
            if (ColaboradorId <= 0)
                return;
            try
            {
                IsBusy = true;
                var colaboradorData = await _colaboradorService.ObterPorIdAsync(ColaboradorId);

                if (colaboradorData != null)

                {
                    Colaborador = colaboradorData;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao carregar colaborador: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
        [RelayCommand]
        public async Task SaveColaboradorAsync()
        {
            if (IsBusy) return;
            if (!ValidateColaborador(Colaborador)) return;

            try
            {
                IsBusy = true;

                // Debug logs
                System.Diagnostics.Debug.WriteLine($"[DEBUG] SaveColaborador - Início");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] SaveColaborador - IsEditMode: {IsEditMode}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] SaveColaborador - ColaboradorId: {ColaboradorId}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] SaveColaborador - Colaborador.Id antes: {Colaborador.Id}");

                // Garantir que o ID está correto para edição
                if (IsEditMode)
                {
                    Colaborador.Id = ColaboradorId;
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] SaveColaborador - Colaborador.Id depois: {Colaborador.Id}");
                }

                // Validar e buscar logradouro
                var logradouroData = await _logradouroService.ObterPorCepAsync(Colaborador.Endereco.Cep);
                if (logradouroData == null)
                {
                    await Shell.Current.DisplayAlert("Erro", "O CEP informado não existe.", "OK");
                    return;
                }

                // Atualizar o logradouro
                Colaborador.Endereco = logradouroData;

                if (IsEditMode)
                {
                    var atualizado = await _colaboradorService.AtualizarAsync(Colaborador);
                    if (atualizado != null)
                    {
                        Colaborador = atualizado;
                        await Shell.Current.DisplayAlert("Sucesso", "Colaborador atualizado com sucesso!", "OK");
                        await Shell.Current.GoToAsync("..");
                    }
                }
                else
                {
                    var criado = await _colaboradorService.AdicionarAsync(Colaborador);
                    if (criado != null)
                    {
                        Colaborador = criado;
                        await Shell.Current.DisplayAlert("Sucesso", "Colaborador criado com sucesso!", "OK");
                        await Shell.Current.GoToAsync("..");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] SaveColaborador - {ex.Message}");
                await Shell.Current.DisplayAlert("Erro", $"Erro ao salvar colaborador: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
        [RelayCommand]
        public async Task SearchByCpfAsync()
        {
            if (string.IsNullOrWhiteSpace(Colaborador.Cpf))
                return;
            try
            {
                IsBusy = true;
                // normaliza para apenas dígitos (o repositório espera dígitos)

                var cpfNormalized = new string(Colaborador.Cpf.Where(char.IsDigit).ToArray());

                var resultados = (await _colaboradorService.ObterPorCpfAsync(cpfNormalized))?.ToList() ?? new List<ColaboradorDTO>();
                if (!resultados.Any())
                {
                    await Shell.Current.DisplayAlert("Aviso", "CPF não encontrado.", "OK"); return;
                }
                if (resultados.Count == 1)
                {
                    Colaborador = resultados.First();
                    IsEditMode = true;
                    await Shell.Current.DisplayAlert("Aviso", "Colaborador já cadastrado! Dados carregados para edição.", "OK"); return;
                }
                // múltiplos resultados -> perguntar ao usuário qual selecionar

                var options = resultados.Select(c => $"{c.Id} - {c.Nome} ({c.Cpf})").ToArray();

                var escolha = await Shell.Current.DisplayActionSheet("Vários colaboradores encontrados", "Cancelar", null, options);
                if (string.IsNullOrWhiteSpace(escolha) || escolha == "Cancelar")
                    return;
                // extrai ID a partir da string selecionada ("{Id} - ...")
                var idStr = escolha.Split('-', 2).FirstOrDefault()?.Trim();
                if (int.TryParse(idStr, out var selId))

                {
                    var selecionado = resultados.FirstOrDefault(c => c.Id == selId);

                    if (selecionado != null)

                    {
                        Colaborador = selecionado;
                        IsEditMode = true;
                        await Shell.Current.DisplayAlert("Aviso", "Colaborador selecionado: dados carregados para edição.", "OK");
                    }
                }
            }
            catch (Exception ex) { await Shell.Current.DisplayAlert("Erro", $"Erro ao buscar CPF: {ex.Message}", "OK"); }
            finally { IsBusy = false; }
        }
        [RelayCommand]
        public async Task SearchByCepAsync()
        {
            if (string.IsNullOrWhiteSpace(Colaborador.Endereco.Cep))
                return;
            try
            {
                IsBusy = true;
                var logradouroData = await _logradouroService.ObterPorCepAsync(Colaborador.Endereco.Cep);

                if (logradouroData != null)

                {
                    Colaborador.Endereco = logradouroData;
                    OnPropertyChanged(nameof(Colaborador));
                    await Shell.Current.DisplayAlert("Aviso", "CEP encontrado! Endereço preenchido automaticamente.", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Aviso", "CEP não encontrado.", "OK");
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
                string escolha = await Shell.Current.DisplayActionSheet("Origem da Imagem", "Cancelar", null, "Galeria", "Câmera");
                FileResult? result = null;
                if (escolha == "Galeria")

                {
                    result = await FilePicker.Default.PickAsync(new PickOptions
                    {
                        PickerTitle = "Selecione uma imagem",
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
                    Colaborador.Foto = new ArquivoDTO { Conteudo = ms.ToArray() };
                    OnPropertyChanged(nameof(Colaborador));
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao selecionar imagem: {ex.Message}", "OK");
            }
        }
        private static bool ValidateColaborador(ColaboradorDTO colaborador)
        {
            const string validationTitle = "Validação";
            if (string.IsNullOrWhiteSpace(colaborador.Nome))
            {
                Shell.Current.DisplayAlert(validationTitle, "Nome é obrigatório.", "OK");
                return false;
            }
            if (string.IsNullOrWhiteSpace(colaborador.Cpf) || colaborador.Cpf.Length != 11)
            {
                Shell.Current.DisplayAlert(validationTitle, "CPF deve ter 11 dígitos.", "OK");
                return false;
            }
            if (colaborador.DataNascimento == default)
            {
                Shell.Current.DisplayAlert(validationTitle, "Data de nascimento é obrigatória.", "OK");
                return false;
            }
            if (string.IsNullOrWhiteSpace(colaborador.Telefone) || colaborador.Telefone.Length != 11)
            {
                Shell.Current.DisplayAlert(validationTitle, "Telefone deve ter 11 dígitos.", "OK");
                return false;
            }
            if (colaborador.Endereco == null)
            {
                Shell.Current.DisplayAlert(validationTitle, "Endereço é obrigatório.", "OK");
                return false;
            }
            if (string.IsNullOrWhiteSpace(colaborador.Numero))
            {
                Shell.Current.DisplayAlert(validationTitle, "Número é obrigatório.", "OK");
                return false;
            }
            if (colaborador.DataAdmissao == default)
            {
                Shell.Current.DisplayAlert(validationTitle, "Data de admissão é obrigatória.", "OK");
                return false;
            }
            return true;
        }
    }
}
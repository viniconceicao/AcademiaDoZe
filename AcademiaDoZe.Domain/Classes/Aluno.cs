﻿// Aluno: Vinicius de Liz da Conceição
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Domain.Exceptions;
using AcademiaDoZe.Domain.Services;
namespace AcademiaDoZe.Domain.Entities
{
    public class Aluno : Pessoa
    {
        // construtor privado para evitar instância direta
        private Aluno(string nome, string cpf, DateOnly dataNascimento, string telefone, string email, Logradouro endereco, string numero, string complemento, string senha, Arquivo foto)
        : base(nome, cpf, dataNascimento, telefone, email, endereco, numero, complemento, senha, foto)
        { }
        // método de fábrica, ponto de entrada para criar um objeto válido
        public static Aluno Criar(string nome, string cpf, DateOnly dataNascimento, string telefone, string email, Logradouro endereco, string numero, string complemento, string senha, Arquivo foto)
        {
            // Validações e normalizações

            if (NormalizadoService.TextoVazioOuNulo(nome)) throw new DomainException("NOME_OBRIGATORIO");

            nome = NormalizadoService.LimparEspacos(nome);
            if (NormalizadoService.TextoVazioOuNulo(cpf)) throw new DomainException("CPF_OBRIGATORIO");
            cpf = NormalizadoService.LimparEDigitos(cpf);
            if (cpf.Length != 11) throw new DomainException("CPF_DIGITOS");
            if (dataNascimento == default) throw new DomainException("DATA_NASCIMENTO_OBRIGATORIO");
            if (dataNascimento > DateOnly.FromDateTime(DateTime.Today.AddYears(-12))) throw new DomainException("DATA_NASCIMENTO_MINIMA_INVALIDA");
            if (NormalizadoService.TextoVazioOuNulo(telefone)) throw new DomainException("TELEFONE_OBRIGATORIO");
            telefone = NormalizadoService.LimparEDigitos(telefone);
            if (telefone.Length != 11) throw new DomainException("TELEFONE_DIGITOS");
            email = NormalizadoService.LimparEspacos(email);
            if (!NormalizadoService.ValidarFormatoEmail(email)) throw new DomainException("EMAIL_FORMATO");
            if (NormalizadoService.TextoVazioOuNulo(senha)) throw new DomainException("SENHA_OBRIGATORIO");
            senha = NormalizadoService.LimparEspacos(senha);
            if (!NormalizadoService.ValidarFormatoSenha(senha)) throw new DomainException("SENHA_FORMATO");
            // if (foto == null) throw new DomainException("FOTO_OBRIGATORIO");
            if (endereco == null) throw new DomainException("LOGRADOURO_OBRIGATORIO");

            if (NormalizadoService.TextoVazioOuNulo(numero)) throw new DomainException("NUMERO_OBRIGATORIO");
            numero = NormalizadoService.LimparEspacos(numero);
            complemento = NormalizadoService.LimparEspacos(complemento);
            // Cpf único - vamos depender da persistência dos dados

            // criação e retorno do objeto
            return new Aluno(nome, cpf, dataNascimento, telefone, email, endereco, numero, complemento, senha, foto);
        }
    }
}
using System;

namespace AutoEngineerBot.Models
{
    public class Produto
    {
        public string Nome { get; set; } = string.Empty;
        public decimal PrecoAtual { get; set; }
        public DateTime DataConsulta { get; set; }

        // Construtor para inicializar nosso "Vetor" de dados
        public Produto(string nome, decimal precoAtual)
        {
            Nome = nome;
            PrecoAtual = precoAtual;
            DataConsulta = DateTime.Now; // O 't' da nossa função P(t)
        }
    }
}
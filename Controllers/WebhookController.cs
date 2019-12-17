using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Dialogflow.V2;
using Google.Protobuf;
using System.IO;
using System.Text;
using Microsoft.Bot.Builder;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebhookApi.Controllers
{
		
	[Route("webhook")]
	public class WebhookController : Controller
	{
		private static readonly JsonParser jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
		[HttpPost]
		public async Task<JsonResult> GetWebhookResponse()
		{
			WebhookRequest request;
			using (var reader = new StreamReader(Request.Body))
			{
				request = jsonParser.Parse<WebhookRequest>(reader);
			}

			//Get week day
			string wk = DateTime.Today.DayOfWeek.ToString();
			string[] daysPossible()
			{
				string day1;
				string day2;
				if (wk == "Saturday" || wk == "Sunday")
				{
					day1 = "segunda-feira";
					day2 = "terça-feira";
					string[] possibleDays = { day1, day2 };
					return possibleDays;
				}
				if (wk == "Monday")
				{
					day1 = "terça-feira";
					day2 = "quarta-feira";
					string[] possibleDays = { day1, day2 };
					return possibleDays;
				}
				if (wk == "Tuesday")
				{
					day1 = "quarta-feira";
					day2 = "quinta-feira";
					string[] possibleDays = { day1, day2 };
					return possibleDays;
				}
				if (wk == "Wednesday")
				{
					day1 = "quinta-feira";
					day2 = "sexta-feira";
					string[] possibleDays = { day1, day2 };
					return possibleDays;
				}
				if (wk == "Thursday")
				{
					day1 = "sexta-feira";
					day2 = "segunda-feira";
					string[] possibleDays = { day1, day2 };
					return possibleDays;
				}
				if (wk == "Friday")
				{
					day1 = "segunda-feira";
					day2 = "terça-feira";
					string[] possibleDays = { day1, day2 };
					return possibleDays;
				}
				string[] wentWrong = { "error", "error2" };
				return wentWrong;
			}

			var canDay1 = daysPossible()[0];
			var canDay2 = daysPossible()[1];

			var pas = request.QueryResult.Parameters;
			var containsDate = pas.Fields.ContainsKey("date") && pas.Fields["date"].ToString().Replace('\"', ' ').Trim().Length > 0;

			string updateAgenda()
			{
				if (containsDate)
				{
					string userDate = pas.Fields["date"].ToString().Replace('\"', ' ').Trim();
					string formatDate() 
					{
						string formatedDate = "";
						for (int i = 0; i < 11; i++)
						{
							formatedDate += userDate[i];
						}
						Console.WriteLine(formatedDate);
						return formatedDate.Replace("-", "").Trim();
					}
					string getYear()
					{
						string year = "";
						for(int i = 0; i < 4; i++)
						{
							year += formatDate()[i];
						}
						Console.WriteLine(year);
						return year;
					}

					string getMonth()
					{
						string month = "";
						for(int i = 4; i < 6; i++)
						{
							month += formatDate()[i];
						}
						Console.WriteLine(month);
						return month;
					}

					string getDay()
					{
						string day = "";
						for(int i = 6; i < 8; i++)
						{
							day += formatDate()[i];
						}
						Console.WriteLine(day);
						return day;
					}

					int correctYear = Int32.Parse(getYear());
					int correctMonth = Int32.Parse(getMonth());
					int correctDay = Int32.Parse(getDay());

					DateTime dateValue = new DateTime(correctYear, correctMonth, correctDay);
					string correctDate = dateValue.ToString("dddd");
					Console.WriteLine(correctDate);
					return correctDate;
				}
				return "[user said an invalid date]";
			}
			string entityDate = pas.Fields["date"].ToString().Replace('\"', ' ').Trim();
			var intent = request.QueryResult.Intent.DisplayName.ToString().ToLower();
			var response = new WebhookResponse();

			StringBuilder sb = new StringBuilder();
			if (request.QueryResult.IntentDetectionConfidence > 0.2)
			{
				switch (intent)
				{
					case "objetivo":
						//sb.Append("Neste momento foram realizadas 35 vendas correspondendo a 50% do objetivo mensal.\nA previsão aponta para a realização de 90% do objetivo.");
						sb.Append("CA");
						break;
					case "objetivometas":
						sb.Append("Com a atual taxa de concretização teremos de realizar cerca de 20 contactos comerciais por dia para atingir o objetivo mensal. Atualmente estamos a realizar cerca de 10 por dia");
						break;
					case "impacto":
						sb.Append("Para tal acontecer a ordem de trabalhos tem de ser alterada alocando 60% do trabalho a Vendas e 40% a Serviços. Deseja atualizar a ordem de trabalhos da equipa comercial?");
						break;
					case "impacto - yes":
						sb.Append("Ok. A equipa será notificada das alterações​. Algo mais em que posso ajudar?");
						break;
					case "melhorcolaboradores":
						sb.Append("A Ana e o Pedro foram os melhores colaboradores na última semana superando os seus objetivos em 15% e 10% respetivamente.");
						break;
					case "restantescolaboradores":
						sb.Append("A Maria concretizou 70% do seu objetivo, o Manuel 85% e o Francisco 90%.\n ​O feedback da equipa comercial indica que a informação sobre o produto é pouco clara. Deseja reforçar a componente de formação sobre o produto para estes colaboradores?");
						break;
					case "restantescolaboradores - yes":
						sb.Append("Ok. A componente de formação para este produto vai ser reforçada para estes colaboradores.\nAlgo mais em que posso ajudar?");
						break;
					case "restantescolaboradores - yes - no":
						sb.Append("Ok. Obrigado.");
						break;
					case "agenda":
						sb.Append("OK. Deseja agendar a reunião para outro dia?");
						break;
					case "agenda - yes":
						sb.Append("Tem conhecimento da disponibilidade do cliente?​");
						break;
					case "possiveis":
						sb.Append($"Encontrei disponíveis os seguinte horários: {canDay1} às 15:00 ou na {canDay2} às 16:00. Qual dos horários deseja?");
						break;
					case "marcaçao":
						sb.Append($"Ok, cancelei a sua reunião de hoje às 15:30 com o cliente João Silva e marquei uma nova reunião para {updateAgenda()} às 15:00. Vou informar o cliente das alterações. Algo mais em que posso ajudar?​");
						break;
					case "horariohoje":
						sb.Append("Para hoje tem agendadas 2 atividades: Pedido de adesão ao CA Online às 14:15 e um contacto comercial  de promoção do Cartão CA Corporate Premium às 15:15 horas. Algo mais em que posso ajudar?");
						break;
					case "not available":
						sb.Append("Desculpe, não percebi o que disse");
						break;
					default:
						sb.Append("Desculpe, não percebi o que disse");
						break;
				}
				response.FulfillmentText = sb.ToString();
				return Json(response);
			}
			else
			{
				sb.Append("Desculpa, não percebi o que disse");
				response.FulfillmentText = sb.ToString();
				return Json(response);
			}

			//if (request.QueryResult.IntentDetectionConfidence < 0.2 || request.QueryResult.LanguageCode == "pt")
			//{
			//	sb.Append("Desculpa, não percebi o que disse");
			//}
			//response.FulfillmentText = sb.ToString();
			//return Json(response);
		}
	}
}
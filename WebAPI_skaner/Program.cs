using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using WebAPI_skaner.Data;
using WebAPI_skaner.Models;

namespace WebAPI_skaner
{
    public class Program
    {
        
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            int catchedId = 0;

            string ConString = "Data Source = LENOVOD\\SQLEXP19; Initial Catalog = HM_EASYSPORT; User ID = sa; Password = Synergi@;";
            string user = "API";
            // Add services to the container.
            //  builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddCors();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true)
            .AllowCredentials());
            }

         

            app.MapGet("/api/coupon", () =>
            {
                return Results.Ok(CouponStore.couponList);
            });

            //app.MapPost("/api/coupon", ([FromBody] Coupon coupon) =>
            //{
            //    return "Succes";
            //});

            app.MapPost("/api/InsertToDB", (string queryToExecute) =>
            {
                SqlConnection sqlCon = new SqlConnection();
                sqlCon.ConnectionString = ConString;
                sqlCon.Open();
                string sqlQuery = queryToExecute;
                string sqlQueryToLog = sqlQuery.Replace("('", "(''").Replace("')", "'')");
                
               // string sqlQuery = FixQuotesInQuery(queryToExecute); zue

                //Firstly Make Log into dbo.[HM.ES_ProMagLog]                    queryString, timeReceived, isExecuted, timeExecuted, username
                ///    string sqlLogQuery = $"INSERT INTO dbo.[HM.ES_ProMagLog] VALUES('{sqlQuery}', GETDATE(), 0, NULL, '{user}')";
                string sqlLogQuery = "INSERT INTO dbo.[HM.ES_ProMagLog] VALUES('" + sqlQueryToLog +"', GETDATE(), 0, NULL, '" + user +"')";


            //    string tstring = FixQuotesInQuery(sqlLogQuery);

                SqlCommand sqlCommand = new SqlCommand(sqlLogQuery, sqlCon);
                try//When failed, make insert into  INSERT INTO dbo.[HM.ES_ProMagLog] VALUES('then Zmiana nie zosta³a naniesiona :)', GETDATE(), 0, NULL, 'Dawid')
                {
                    int result = sqlCommand.ExecuteNonQuery();
                    
                }
                catch(Exception ex)
                {
                    string sqlErrorLog = $"INSERT INTO dbo.HM.ES_ProMagErrorLog VALUES('{sqlQueryToLog}', GETDATE(), 0, NULL, '{user}', '{ex.ToString()}')";
                    sqlCommand.CommandText = sqlErrorLog;
                    sqlCommand.ExecuteNonQuery();
                }

                sqlCommand.CommandText = queryToExecute;
                int rowsAffected = sqlCommand.ExecuteNonQuery();
                if (rowsAffected == 0)
                {
                    sqlCon.Close();
                    return "0 rows affected";
                }
                else
                {

                    return rowsAffected + "  rows affected";
                }
               // return a + "NIE";
            });


           // app.MapPost("/api/Login", ([FromBody] LoginModel loginCredentials)=>
           app.MapPost("/api/Login", ([FromBody] LoginModel loginCredentials)=> 
            {
                string login = loginCredentials.Login;
                string password = loginCredentials.Password;

                SqlConnection sqlConnection = new SqlConnection();
                sqlConnection.ConnectionString = ConString; 
                sqlConnection.Open();

                SqlCommand sqlCommand = new SqlCommand();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = "SELECT Count(*) FROM [HM].[PROMAG_USERS] WHERE Login ='" + login + "' AND Password ='" + password + "' AND ACTIVE=1";

                var response = sqlCommand.ExecuteScalar();
                //return sqlCommand.CommandText;
                return response.ToString();

            });



            app.MapPut("/api/LoginPut", (LoginModel loginCredentials) =>
            {
                string login = loginCredentials.Login;
                string password = loginCredentials.Password;

                SqlConnection sqlConnection = new SqlConnection();
                sqlConnection.ConnectionString = ConString;
                sqlConnection.Open();

                SqlCommand sqlCommand = new SqlCommand();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = "SELECT Count(*) FROM [HM].[PROMAG_USERS] WHERE Login ='" + login + "' AND Password ='" + password + "' AND ACTIVE=1";

                var response = sqlCommand.ExecuteScalar();

                return new HttpResponseMessage(System.Net.HttpStatusCode.PartialContent);
            });



            app.UseHttpsRedirection();
            app.Run();
        }
    }
}
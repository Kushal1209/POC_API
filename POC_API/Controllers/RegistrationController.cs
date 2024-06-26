﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using POC_API.DTOs;
using POC_API.Models;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace POC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {

        private readonly IConfiguration _context;

        public RegistrationController(IConfiguration context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("Registration")]
        public Response Register(RegistratioinDTO registrationDTO)
        {
            Response response = new Response();
            SqlConnection con = new SqlConnection(_context.GetConnectionString("Poc_api").ToString());

            SqlCommand cmd = new SqlCommand("Insert into Registration (UserName,Password,Email,IsActive) values ('" + registrationDTO.UserName + "','" + registrationDTO.Password + "','" + registrationDTO.Email + "','" + registrationDTO.IsActive + "')", con);
            con.Open();
            int i = cmd.ExecuteNonQuery();
            con.Close();
            if (i > 0)
            {
                response.StatusCode = 200;
                response.StatusMessage = "OK";
                return response;
            }
            else
            {
                response.StatusCode = 400;
                response.StatusMessage = "Error";
                return response;
            }

        }

        [HttpPost]
        [Route("login")]
        public Response login(LoginDTO loginDTO)
        {
            Response response = new Response();

            SqlConnection con = new SqlConnection(_context.GetConnectionString("Poc_api").ToString());

            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter("Select * from Registration where Email = '" + loginDTO.Email + "' And Password = '" + loginDTO.Password + "' And IsActive = 1 ", con);

            DataTable dataTable = new DataTable();

            sqlDataAdapter.Fill(dataTable);

            if (dataTable.Rows.Count > 0)
            {
                response.StatusCode = 200;
                response.StatusMessage = "OK";
                return response;
            }
            else
            {
                response.StatusCode = 400;
                response.StatusMessage = "Error";
                return response;
            }

        }
    }
}

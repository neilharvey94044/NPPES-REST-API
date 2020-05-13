using System;
using System.Security;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using System.Data;
using NppesAPI.Models;
using NppesAPI.SQL;

namespace NppesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NppesController : ControllerBase
    {

        ILogger<NppesController> _logger { get; }
        IConfiguration _config { get; }
        IWebHostEnvironment _env { get; }
        SqlCredential _sqlcred { get; }
        string _connstr { get; }

        public NppesController(IConfiguration config, IWebHostEnvironment env, ILogger<NppesController> logger)
        {
            _logger = logger;
            _config = config;
            _env = env;

            // Create SQL Server credential
            string txtPwd = config["Sqlserver:password"];
            if (txtPwd == null)
            {
                string errmsg = "Unable to obtain SQL Server Password from application configuration - Exiting Program";
                _logger.LogError(errmsg);
                throw new System.ArgumentNullException("Sqlserver:password", errmsg);
            }
            SecureString secureStringPwd = new SecureString();
            txtPwd.ToCharArray().ToList().ForEach(p => secureStringPwd.AppendChar(p));
            secureStringPwd.MakeReadOnly();
            string txtUserId = config["Sqlserver:userid"];
            if (txtUserId == null)
            {
                string errmsg = "Unable to obtain SQL Server User ID from application configuration - Exiting Program";
                _logger.LogError(errmsg);
                throw new System.ArgumentNullException("Sqlserver:userid", errmsg);
            }
            _sqlcred = new SqlCredential(txtUserId, secureStringPwd);

            // Get SQL Server connection string
            _connstr = _config.GetSection("ConnectionStrings")["DefaultConnection"];

        }

        /* GET: api/Nppes
         * Returns a set of Providers matching query parameters.
         * Uses a SqlDataAdapter and DataSet to obtain data by 
         * invoking a stored procedure.
         */

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<IEnumerable<Provider>>> GetProviders(string taxonomycode, Decimal zipcode, int distance, int type)
        {


            _logger.LogInformation("Getting Providers with zipcode {zipcode} distance {distance} type {type}", zipcode, distance, type);
            using (SqlConnection connection = new SqlConnection(_connstr, _sqlcred))
            {
                // Get the Providers
                SqlDataAdapter adapter = new SqlDataAdapter();
                SqlCommand command = new SqlCommand(SQLCommands.NPPESSQL_0015, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@distance", SqlDbType.Int).Value = distance;
                command.Parameters.Add("@zipcode", SqlDbType.Decimal).Value = zipcode;
                adapter.SelectCommand = command;

                // Fill the DataSet.
                DataSet dataSet = new DataSet("Results");
                await Task.Run(() => adapter.Fill(dataSet));
                var prov = Provider.CreateDirector().CreateProviders(dataSet);

                if (prov.Count() > 0)
                    return Ok(prov);
                else
                    return NoContent();
            }
        }


        /* GET: api/Nppes/<npi>
         * Returns a single Provider or 404.
         * Uses a SqlDataReader to obtain data.
         * 
         */
        [HttpGet("{npi}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Provider>> GetProvider(Decimal npi)
        {
            _logger.LogInformation("Getting Provider {npi} from database", npi);
            using (SqlConnection connection = new SqlConnection(_connstr, _sqlcred))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(SQLCommands.NPPESSQL_0016, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@npi", SqlDbType.Decimal).Value = npi;
                SqlDataReader reader = await command.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    //SQLutils.logDebugSchema(reader, _logger);
                    await reader.ReadAsync();
                    Provider prov = Provider.CreateDirector().CreateProvider(reader);

                    return prov;
                }
                else
                {
                    _logger.LogError("Provider ID not found {npi}", npi);
                    return NotFound(npi);
                }

            }
        }
    }
}

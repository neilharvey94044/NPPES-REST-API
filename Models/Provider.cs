using Microsoft.AspNetCore.Http;
using NLog.LayoutRenderers;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using NppesAPI.SQL;
using Microsoft.Data.SqlClient.DataClassification;
using Newtonsoft.Json;

namespace NppesAPI.Models
{
    public class Provider
    {
        public enum NPIType { Individual = 1, Organization = 2 }
        [JsonProperty (Order = -2)]
        public string NPI { get; protected set; }
        
        public NPIType Type;
        public string Name { get; protected set; }
        public string AddrLine1 { get; protected set; }
        public string AddrLine2 { get; protected set; }
        public string AddrCity { get; protected set; }
        public string AddrState { get; protected set; }
        public string AddrZip { get; protected set; }

        [JsonProperty(Order = 1)]
        public List<Taxonomy> taxonomies = new List<Taxonomy>();

        [JsonProperty(Order = 2)]
        public List<OthProvider> othProviders = new List<OthProvider>();

        private Provider() { }

       

        public static Director CreateDirector() { return new Director(); }
        public static Builder CreateBuilder() { return new Builder(); }

        //===========================================================================================
        public class Taxonomy
        {
            public string TxCode { get; set; }
            public string Classification { get; set; }
            public string Specialization { get; set; }
        }
        public class OthProvider
        {
            public string OthId { get; set; }
            public string OthTypeCode { get; set; }
            public string OthIssuer { get; set; }
            public string OthState { get; set; }

        }

        //===========================================================================================

        public class Director
        {
            public IEnumerable<Provider> CreateProviders(DataSet ds)
            {
                List<Provider> providers = new List<Provider>();

                DataTable dt = ds.Tables["Table"];   dt.TableName  = "Provider";
                DataTable dt1 = ds.Tables["Table1"]; dt1.TableName = "Taxonomy";
                DataTable dt2 = ds.Tables["Table2"]; dt2.TableName = "OthProvider";

                // Setup Relations
                DataColumn parentColumn =
                    ds.Tables["Provider"].Columns["NPI"];
                DataColumn txchildColumn =
                    ds.Tables["Taxonomy"].Columns["NPI"];
                DataColumn othchildColumn =
                    ds.Tables["OthProvider"].Columns["NPI"];
                DataRelation txrelation =
                    new System.Data.DataRelation("ProvidersTaxonomy",
                    parentColumn, txchildColumn);
                DataRelation othrelation =
                    new System.Data.DataRelation("ProvidersOth",
                    parentColumn, othchildColumn);
                ds.Relations.Add(txrelation);
                ds.Relations.Add(othrelation);

                foreach (DataRow row in dt.Rows)
                {

                    // Get Taxonomy
                    DataRow[] txitems = row.GetChildRows(txrelation);

                    // Get Other Related Providers
                    DataRow[] othitems = row.GetChildRows(othrelation);

                    // Create a Provider
                    Provider prov = CreateProvider(row, txitems, othitems);
                    // Add the Provider to a collection
                    providers.Add(prov);

                }

                return providers;
            }

            /*
             * Create a Provider using results from a DataSet
             * 
             */
            public Provider CreateProvider(DataRow parent, DataRow[] tx, DataRow[] oth)
            {

                // values
                Provider.NPIType _type = (Provider.NPIType)parent["Type"];
                string _name;

                // Create a Name field that is either an Individual (i.e. Physician) or a Company/Org
                if (_type == Provider.NPIType.Individual)
                {
                    _name = parent["NamePrefix"] + " "
                                + parent["NameFirst"] + " "
                                + parent["NameLast"] + " "
                                + parent["NameSuffix"];
                    ;
                }
                else _name = parent["OrgName"].ToString();

                // create the Provider using a Builder
                Provider.Builder provbuilder = Provider.CreateBuilder()
                    .NPI(parent["NPI"].ToString())
                    .Type(_type)
                    .Name(_name)
                    .AddrLine1(parent["AddrLine1"].ToString())
                    .AddrLine2(parent["AddrLine2"].ToString())
                    .AddrCity(parent["AddrCity"].ToString())
                    .AddrState(parent["AddrState"].ToString())
                    .AddrZip(parent["AddrZip"].ToString());

                // Add the Taxonomy for this Provider
                foreach (DataRow row in tx)
                {
                    provbuilder.AddTx(new Taxonomy()
                                    {
                                        TxCode = row["TxCode"].ToString(),
                                        Classification = row["Classification"].ToString(),
                                        Specialization = row["Specialization"].ToString()
                                    }
                    );
                }

                // Add the Other Provider Relationships
                foreach (DataRow row in oth)
                {
                    provbuilder.AddOth(new OthProvider()
                                        {   OthId = row["OthId"].ToString(),
                                            OthTypeCode = row["OthTypeCode"].ToString(),
                                            OthIssuer = row["OthIssuer"].ToString(),
                                            OthState = row["OthState"].ToString()
                                        }
                    );
                }

                Provider prov = provbuilder.build();

                return prov;
            }



            /*
             * Create a single Provider using results from a SqlDataReader
             * 
             */
            public Provider CreateProvider(SqlDataReader rdr)
            {
           
                // values
                int _NPI = rdr.GetOrdinal("NPI");
                int _Type = rdr.GetOrdinal("Type");
                Provider.NPIType _type = (Provider.NPIType) rdr.GetInt32(_Type);
                string _name;

                // Create a Name field that is either an Individual (i.e. Physician) or a Company/Org
                if (_type == Provider.NPIType.Individual)
                {
                    _name = rdr.GetColString("NamePrefix") + " "
                                + rdr.GetColString("NameFirst") + " "
                                + rdr.GetColString("NameLast") + " "
                                + rdr.GetColString("NameSuffix");
                            ;
                }
                else _name = rdr.GetColString("OrgName");

                // create the Provider using a Builder
                Provider.Builder provbuilder = Provider.CreateBuilder()
                    .NPI(rdr.GetValue(_NPI).ToString())
                    .Type(_type)
                    .Name(_name)
                    .AddrLine1(rdr.GetColString("AddrLine1"))
                    .AddrLine2(rdr.GetColString("AddrLine2"))
                    .AddrCity(rdr.GetColString("AddrCity"))
                    .AddrState(rdr.GetColString("AddrState"))
                    .AddrZip(rdr.GetColString("AddrZip"));

                // get Taxonomy
                rdr.NextResult();
                while(rdr.Read()){
                    provbuilder.AddTx(new Taxonomy()
                                        {
                                            TxCode = rdr.GetColString("TxCode"),
                                            Classification = rdr.GetColString("Classification"),
                                            Specialization = rdr.GetColString("Specialization")
                                        }
                    );
                }

                // get Other Provider Relationships
                rdr.NextResult();
                while(rdr.Read()) {
                    provbuilder.AddOth(new OthProvider()
                                            {
                                                OthId = rdr.GetColString("OthId"),
                                                OthTypeCode = rdr.GetColString("OthTypeCode"),
                                                OthIssuer = rdr.GetColString("OthIssuer"),
                                                OthState = rdr.GetColString("OthState")
                                            }
                                            );
                }


                Provider prov = provbuilder.build();

                return prov;
            }

        }

        //===========================================================================================

        public class Builder
        {
            private Provider _provider;

            public Builder()
            {
                _provider = new Provider();
            }

            public Builder NPI(string NPI)
            {
                _provider.NPI = NPI;
                return this;
            }

            public Builder Type(Provider.NPIType Type)
            {
                _provider.Type = Type;
                return this;
            }

            public Builder Name(string Name)
            {
                _provider.Name = Name;
                return this;
            }

            public Builder AddrLine1(string AddrLine1)
            {
                _provider.AddrLine1 = AddrLine1;
                return this;
            }

            public Builder AddrLine2(string AddrLine2)
            {
                _provider.AddrLine2 = AddrLine2;
                return this;
            }

            public Builder AddrCity(string AddrCity)
            {
                _provider.AddrCity = AddrCity;
                return this;
            }
            public Builder AddrState(string AddrState)
            {
                _provider.AddrState = AddrState;
                return this;
            }

            public Builder AddrZip(string AddrZip)
            {
                _provider.AddrZip = AddrZip;
                return this;
            }

            public void AddTx(Taxonomy tx)
            {
                _provider.taxonomies.Add(tx);
            }
            public void AddOth(OthProvider oth)
            {
                _provider.othProviders.Add(oth);
            }

            public Provider build()
            {
                return _provider;
            }

        }
    }
}


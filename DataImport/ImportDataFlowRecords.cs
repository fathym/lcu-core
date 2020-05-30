using LCU;
using LCU.Graphs;
using LCU.Graphs.Registry.Enterprises.DataFlows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace DataImport
{
    [TestClass]
    public class ImportDataFlowRecords
    {
        #region Fields
        protected readonly string entLookup;

        protected readonly ApplicationProfileManager appProfileMgr;

        protected readonly GremlinClientPoolManager gremlinMgr;

        protected readonly LCUGraphConfig lcuGraphConfig;

        protected readonly SchemaFunctionDefinitionGraph schemaFunctionDefGraph;

        protected readonly TypeDefinitionGraph typeDefGraph;

        #endregion

        #region Constructors
        public ImportDataFlowRecords()
        {
            this.entLookup = "3ebd1c0d-22d0-489e-a46f-3260103c8cd7";

            this.appProfileMgr = new ApplicationProfileManager(32, 4, 60);

            //INT
            this.lcuGraphConfig = new LCUGraphConfig()
            {
                APIKey = "Qan4GIhRTovYZmdogrsZnYDR8xiI0RL6gvGa9ufnp3c4RGRit0xAmasqiEOEwqPQs3HoN60w8NyM5E6DbW96yA==",
                Host = "lcu-int.gremlin.cosmosdb.azure.com",
                Database = "Registry",
                Graph = "Enterprises"
            };

            //PROD
            //this.lcuGraphConfig = new LCUGraphConfig()
            //{
            //    APIKey = "LzJTUcfxhasgH9WsarCPdiDnGJS162DivRPPcBUxeC7WkPnqMGa31kzzWX1EPm5ZeO2mEyUxMXUIoKg7THU8VQ==",
            //    Host = "lcu-prd.gremlin.cosmosdb.azure.com",
            //    Database = "Registry",
            //    Graph = "Enterprises"
            //};

            this.gremlinMgr = new GremlinClientPoolManager(appProfileMgr, lcuGraphConfig);

            this.schemaFunctionDefGraph = new SchemaFunctionDefinitionGraph(gremlinMgr);

            this.typeDefGraph = new TypeDefinitionGraph(gremlinMgr);
        }
        #endregion

        #region API Methods
        //[TestMethod]
        public void ImportSchemaFunctionDefinitions()
        {
            var items = loadSchemaFunctionDefinitionsFromJSON();

            items.ForEach(
                (item) =>
                {
                    var sfDef = schemaFunctionDefGraph.SaveSchemaFunctionDefinitionDefinition(entLookup, item).GetAwaiter().GetResult();
                });
        }

        //[TestMethod]
        public void ImportTypeDefinitions()
        {
            var items = loadTypeDefinitionsFromJSON();

            items.ForEach(
                (item) =>
                {
                    var typeDef = typeDefGraph.SaveTypeDefinition(entLookup, item).GetAwaiter().GetResult();
                });
        }
        #endregion

        #region Helpers
        protected List<SchemaFunctionDefinition> loadSchemaFunctionDefinitionsFromJSON()
        {
            var stringContent = "[{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"Concatenation Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Standard\"," +
                                    "    \"MinProperties\": 2," +
                                    "    \"MaxProperties\": 5," +
                                    "    \"Name\": \"Concat\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-431d-933f-243c2cc2c2b8\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_Concat\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"string\"" +
                                    "    ]," +
                                    "    \"AllowDifferentIncommingTypes\": false," +
                                    "    \"AllowMultipleIncomming\": true," +
                                    "    \"ReturnType\": \"string\"," +
                                    "    \"SQL\": \"CONCAT({N},)\"," +
                                    "" +
                                    "    \"_ts\": 1523564060" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"ToString Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Standard\"," +
                                    "    \"MinProperties\": 1," +
                                    "    \"MaxProperties\": 1," +
                                    "    \"Name\": \"ToString\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-431d-933f-243c2cc2c2b9\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_ToString\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"boolean\"," +
                                    "        \"object\"," +
                                    "        \"null\"," +
                                    "        \"integer\"," +
                                    "        \"number\"," +
                                    "        \"double\"," +
                                    "        \"float\"," +
                                    "        \"datetime\"" +
                                    "    ]," +
                                    "    \"SQL\": \"TRY_CAST({0} AS nvarchar(max))\"," +
                                    "    \"AllowDifferentIncommingTypes\": false," +
                                    "    \"AllowMultipleIncomming\": false," +
                                    "    \"ReturnType\": \"string\"," +
                                    "" +
                                    "    \"_ts\": 1533002771" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"Greater Than Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Comparison\"," +
                                    "    \"MinProperties\": 2," +
                                    "    \"MaxProperties\": 2," +
                                    "    \"Name\": \">\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-431d-933f-243c2cc2c2c1\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_GreaterThan\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"boolean\"," +
                                    "        \"object\"," +
                                    "        \"null\"," +
                                    "        \"notnull\"," +
                                    "        \"integer\"," +
                                    "        \"number\"," +
                                    "        \"double\"," +
                                    "        \"float\"," +
                                    "        \"string\"," +
                                    "        \"datetime\"" +
                                    "    ]," +
                                    "    \"AllowDifferentIncommingTypes\": false," +
                                    "    \"AllowMultipleIncomming\": true," +
                                    "    \"ReturnType\": \"boolean\"," +
                                    "    \"SQL\": \"CASE WHEN {0} > {1} THEN {true} ELSE {false} END\"," +
                                    "    \"SQLBoolean\": \"{0} > {1}\"," +
                                    "" +
                                    "    \"_ts\": 1530910320" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"Greater Than Equal Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Comparison\"," +
                                    "    \"MinProperties\": 2," +
                                    "    \"MaxProperties\": 2," +
                                    "    \"Name\": \">=\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-431d-933f-243c2cc2c2c2\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_GreaterThanEqual\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"boolean\"," +
                                    "        \"object\"," +
                                    "        \"null\"," +
                                    "        \"notnull\"," +
                                    "        \"integer\"," +
                                    "        \"number\"," +
                                    "        \"double\"," +
                                    "        \"float\"," +
                                    "        \"string\"" +
                                    "    ]," +
                                    "    \"AllowDifferentIncommingTypes\": false," +
                                    "    \"AllowMultipleIncomming\": true," +
                                    "    \"ReturnType\": \"boolean\"," +
                                    "    \"SQL\": \"CASE WHEN {0} >= {1} THEN {true} ELSE {false} END\"," +
                                    "    \"SQLBoolean\": \"{0} >= {1}\"," +
                                    "" +
                                    "    \"_ts\": 1518478812" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"Less Than Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Comparison\"," +
                                    "    \"MinProperties\": 2," +
                                    "    \"MaxProperties\": 2," +
                                    "    \"Name\": \"<\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-431d-933f-243c2cc2c2c3\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_LessThan\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"boolean\"," +
                                    "        \"object\"," +
                                    "        \"null\"," +
                                    "        \"notnull\"," +
                                    "        \"integer\"," +
                                    "        \"number\"," +
                                    "        \"double\"," +
                                    "        \"float\"," +
                                    "        \"string\"" +
                                    "    ]," +
                                    "    \"AllowDifferentIncommingTypes\": false," +
                                    "    \"AllowMultipleIncomming\": true," +
                                    "    \"ReturnType\": \"boolean\"," +
                                    "    \"SQL\": \"CASE WHEN {0} < {1} THEN {true} ELSE {false} END\"," +
                                    "    \"SQLBoolean\": \"{0} < {1}\"," +
                                    "" +
                                    "    \"_ts\": 1518478819" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"Less Than Equal Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Comparison\"," +
                                    "    \"MinProperties\": 2," +
                                    "    \"MaxProperties\": 2," +
                                    "    \"Name\": \"<=\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-431d-933f-243c2cc2c2c4\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_LessThanEqual\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"boolean\"," +
                                    "        \"object\"," +
                                    "        \"null\"," +
                                    "        \"notnull\"," +
                                    "        \"integer\"," +
                                    "        \"number\"," +
                                    "        \"double\"," +
                                    "        \"float\"," +
                                    "        \"string\"" +
                                    "    ]," +
                                    "    \"AllowDifferentIncommingTypes\": false," +
                                    "    \"AllowMultipleIncomming\": true," +
                                    "    \"ReturnType\": \"boolean\"," +
                                    "    \"SQL\": \"CASE WHEN {0} <= {1} THEN {true} ELSE {false} END\"," +
                                    "    \"SQLBoolean\": \"{0} <= {1}\"," +
                                    "" +
                                    "    \"_ts\": 1518478828" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"Equal Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Comparison\"," +
                                    "    \"MinProperties\": 2," +
                                    "    \"MaxProperties\": 2," +
                                    "    \"Name\": \"==\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-431d-933f-243c2cc2c2c5\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_Equal\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"boolean\"," +
                                    "        \"object\"," +
                                    "        \"null\"," +
                                    "        \"notnull\"," +
                                    "        \"integer\"," +
                                    "        \"number\"," +
                                    "        \"double\"," +
                                    "        \"float\"," +
                                    "        \"string\"" +
                                    "    ]," +
                                    "    \"AllowDifferentIncommingTypes\": false," +
                                    "    \"AllowMultipleIncomming\": true," +
                                    "    \"ReturnType\": \"boolean\"," +
                                    "    \"SQL\": \"CASE WHEN {0} = {1} THEN {true} ELSE {false} END\"," +
                                    "    \"SQLBoolean\": \"{0} = {1}\"," +
                                    "" +
                                    "    \"_ts\": 1518478841" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"Not Equal Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Comparison\"," +
                                    "    \"MinProperties\": 2," +
                                    "    \"MaxProperties\": 2," +
                                    "    \"Name\": \"!=\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-431d-933f-243c2cc2c2c6\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_NotEqual\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"boolean\"," +
                                    "        \"object\"," +
                                    "        \"null\"," +
                                    "        \"notnull\"," +
                                    "        \"integer\"," +
                                    "        \"number\"," +
                                    "        \"double\"," +
                                    "        \"float\"," +
                                    "        \"string\"" +
                                    "    ]," +
                                    "    \"AllowDifferentIncommingTypes\": false," +
                                    "    \"AllowMultipleIncomming\": true," +
                                    "    \"ReturnType\": \"boolean\"," +
                                    "    \"SQL\": \"CASE WHEN {0} != {1} THEN {true} ELSE {false} END\"," +
                                    "    \"SQLBoolean\": \"{0} != {1}\"," +
                                    "" +
                                    "    \"_ts\": 1518478849" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"AND Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Conditional\"," +
                                    "    \"MinProperties\": 2," +
                                    "    \"MaxProperties\": 10," +
                                    "    \"Name\": \"AND\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-431d-933f-243c2cc2c3c6\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_AND\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"boolean\"" +
                                    "    ]," +
                                    "    \"AllowDifferentIncommingTypes\": false," +
                                    "    \"AllowMultipleIncomming\": true," +
                                    "    \"ReturnType\": \"boolean\"," +
                                    "    \"SQL\": \"CASE WHEN {0} AND {1} THEN {true} ELSE {false} END\"," +
                                    "    \"SQLBoolean\": \"{0} AND {1}\"," +
                                    "" +
                                    "    \"_ts\": 1518478888" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"OR Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Conditional\"," +
                                    "    \"MinProperties\": 2," +
                                    "    \"MaxProperties\": 10," +
                                    "    \"Name\": \"OR\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-431d-933f-243c2cc2c4c6\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_OR\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"boolean\"" +
                                    "    ]," +
                                    "    \"AllowDifferentIncommingTypes\": false," +
                                    "    \"AllowMultipleIncomming\": true," +
                                    "    \"ReturnType\": \"boolean\"," +
                                    "    \"SQL\": \"CASE WHEN {0} OR {1} THEN {true} ELSE {false} END\"," +
                                    "    \"SQLBoolean\": \"{0} OR {1}\"," +
                                    "" +
                                    "    \"_ts\": 1518478896" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"ToInt Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Standard\"," +
                                    "    \"MinProperties\": 1," +
                                    "    \"MaxProperties\": 1," +
                                    "    \"Name\": \"ToInt\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-431d-933f-243c2cc2c5c6\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_ToInt\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"boolean\"," +
                                    "        \"string\"," +
                                    "        \"double\"," +
                                    "        \"float\"," +
                                    "        \"number\"" +
                                    "    ]," +
                                    "    \"SQL\": \"TRY_CAST({0} AS bigint)\"," +
                                    "    \"AllowDifferentIncommingTypes\": false," +
                                    "    \"AllowMultipleIncomming\": false," +
                                    "    \"ReturnType\": \"integer\"," +
                                    "" +
                                    "    \"_ts\": 1533002792" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"Trim Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Standard\"," +
                                    "    \"MinProperties\": 1," +
                                    "    \"MaxProperties\": 1," +
                                    "    \"Name\": \"Trim\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-431d-933f-243c2cc2c6c6\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_Trim\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"string\"" +
                                    "    ]," +
                                    "    \"AllowDifferentIncommingTypes\": false," +
                                    "    \"AllowMultipleIncomming\": false," +
                                    "    \"ReturnType\": \"string\"," +
                                    "    \"SQL\": \"TRIM({0})\"," +
                                    "" +
                                    "    \"_ts\": 1514602534" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"ToDouble Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Standard\"," +
                                    "    \"MinProperties\": 1," +
                                    "    \"MaxProperties\": 1," +
                                    "    \"Name\": \"ToDouble\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-431d-933f-243c2cc2c7c6\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_ToDouble\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"string\"," +
                                    "        \"boolean\"," +
                                    "        \"integer\"," +
                                    "        \"float\"," +
                                    "        \"number\"" +
                                    "    ]," +
                                    "    \"SQL\": \"TRY_CAST({0} AS float)\"," +
                                    "    \"AllowDifferentIncommingTypes\": false," +
                                    "    \"AllowMultipleIncomming\": false," +
                                    "    \"ReturnType\": \"double\"," +
                                    "" +
                                    "    \"_ts\": 1533002837" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"ToFloat Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Standard\"," +
                                    "    \"MinProperties\": 1," +
                                    "    \"MaxProperties\": 1," +
                                    "    \"Name\": \"ToFloat\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-431d-933f-243c2cc2c8c6\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_ToFloat\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"string\"," +
                                    "        \"boolean\"," +
                                    "        \"integer\"," +
                                    "        \"double\"," +
                                    "        \"number\"" +
                                    "    ]," +
                                    "    \"SQL\": \"TRY_CAST({0} AS float)\"," +
                                    "    \"AllowDifferentIncommingTypes\": false," +
                                    "    \"AllowMultipleIncomming\": false," +
                                    "    \"ReturnType\": \"float\"," +
                                    "" +
                                    "    \"_ts\": 1513834253" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"ToBoolean Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Standard\"," +
                                    "    \"MinProperties\": 1," +
                                    "    \"MaxProperties\": 1," +
                                    "    \"Name\": \"ToBoolean\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-431d-933f-243c2cc2c9c6\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_ToBoolean\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"string\"," +
                                    "        \"integer\"," +
                                    "        \"double\"," +
                                    "        \"number\"," +
                                    "        \"null\"," +
                                    "        \"notnull\"," +
                                    "        \"object\"" +
                                    "    ]," +
                                    "    \"SQL\": \"TRY_CAST({0} AS bigint)\"," +
                                    "    \"AllowDifferentIncommingTypes\": false," +
                                    "    \"AllowMultipleIncomming\": false," +
                                    "    \"ReturnType\": \"boolean\"," +
                                    "" +
                                    "    \"_ts\": 1533002858" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"Sum (Scalar) Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Standard\"," +
                                    "    \"MinProperties\": 1," +
                                    "    \"MaxProperties\": 15," +
                                    "    \"Name\": \"Sum (Scalar)\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-431d-933f-343c2cc2c9c6\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_Sum\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"integer\"," +
                                    "        \"double\"," +
                                    "        \"number\"," +
                                    "        \"float\"" +
                                    "    ]," +
                                    "    \"AllowDifferentIncommingTypes\": true," +
                                    "    \"AllowMultipleIncomming\": true," +
                                    "    \"ReturnType\": \"double\"," +
                                    "    \"SQL\": \"({N+})\"," +
                                    "" +
                                    "    \"_ts\": 1531169568" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"Subtract Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Standard\"," +
                                    "    \"MinProperties\": 2," +
                                    "    \"MaxProperties\": 2," +
                                    "    \"Name\": \"Subtract\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-431d-933f-443c2cc2c9c6\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_Subtract\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"integer\"," +
                                    "        \"double\"," +
                                    "        \"number\"," +
                                    "        \"float\"" +
                                    "    ]," +
                                    "    \"AllowDifferentIncommingTypes\": true," +
                                    "    \"AllowMultipleIncomming\": true," +
                                    "    \"ReturnType\": \"double\"," +
                                    "    \"SQL\": \"({N-})\"," +
                                    "" +
                                    "    \"_ts\": 1514602737" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"Multiply Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Standard\"," +
                                    "    \"MinProperties\": 2," +
                                    "    \"MaxProperties\": 15," +
                                    "    \"Name\": \"Multiply\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-431d-933f-543c2cc2c9c6\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_Multiply\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"integer\"," +
                                    "        \"double\"," +
                                    "        \"number\"," +
                                    "        \"float\"" +
                                    "    ]," +
                                    "    \"AllowDifferentIncommingTypes\": true," +
                                    "    \"AllowMultipleIncomming\": true," +
                                    "    \"ReturnType\": \"double\"," +
                                    "    \"SQL\": \"({N*})\"," +
                                    "" +
                                    "    \"_ts\": 1514602762" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"Divide Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Standard\"," +
                                    "    \"MinProperties\": 2," +
                                    "    \"MaxProperties\": 2," +
                                    "    \"Name\": \"Divide\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-531d-933f-543c2cc2c9c6\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_Divide\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"integer\"," +
                                    "        \"double\"," +
                                    "        \"number\"," +
                                    "        \"float\"" +
                                    "    ]," +
                                    "    \"AllowDifferentIncommingTypes\": true," +
                                    "    \"AllowMultipleIncomming\": true," +
                                    "    \"ReturnType\": \"double\"," +
                                    "    \"SQL\": \"({N/})\"," +
                                    "" +
                                    "    \"_ts\": 1514602774" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"Average (Scalar) Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Standard\"," +
                                    "    \"MinProperties\": 2," +
                                    "    \"MaxProperties\": 15," +
                                    "    \"Name\": \"Average (Scalar)\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-531d-933f-643c2cc2c9c6\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_Average\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"integer\"," +
                                    "        \"double\"," +
                                    "        \"number\"," +
                                    "        \"float\"" +
                                    "    ]," +
                                    "    \"AllowDifferentIncommingTypes\": true," +
                                    "    \"AllowMultipleIncomming\": true," +
                                    "    \"ReturnType\": \"double\"," +
                                    "    \"SQL\": \"(({N+})/{C})\"," +
                                    "" +
                                    "    \"_ts\": 1531169599" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"Length Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Standard\"," +
                                    "    \"MinProperties\": 1," +
                                    "    \"MaxProperties\": 1," +
                                    "    \"Name\": \"Length\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-531d-933f-743c2cc2c9c6\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_Length\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"string\"," +
                                    "        \"array\"" +
                                    "    ]," +
                                    "    \"AllowDifferentIncommingTypes\": false," +
                                    "    \"AllowMultipleIncomming\": false," +
                                    "    \"ReturnType\": \"integer\"," +
                                    "    \"SQL\": \"LEN({0})\"," +
                                    "" +
                                    "    \"_ts\": 1514602934" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"RegEx Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"RegEx\"," +
                                    "    \"MinProperties\": 2," +
                                    "    \"MaxProperties\": 2," +
                                    "    \"Name\": \"RegEx\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-531d-933f-843c2cc2c9c6\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_RegEx\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"string\"" +
                                    "    ]," +
                                    "    \"AllowDifferentIncommingTypes\": false," +
                                    "    \"AllowMultipleIncomming\": false," +
                                    "    \"ReturnType\": \"boolean\"," +
                                    "    \"SQL\": \"REGEXMATCH({0},{1})\"," +
                                    "" +
                                    "    \"_ts\": 1533010140" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"IS NULL Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Standard\"," +
                                    "    \"MinProperties\": 1," +
                                    "    \"MaxProperties\": 1," +
                                    "    \"Name\": \"IS NULL\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-431d-933f-244c2cc2c2b9\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_ISNULL\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"object\"," +
                                    "        \"integer\"," +
                                    "        \"number\"," +
                                    "        \"double\"," +
                                    "        \"float\"," +
                                    "        \"array\"," +
                                    "        \"string\"," +
                                    "        \"datetime\"" +
                                    "    ]," +
                                    "    \"AllowDifferentIncommingTypes\": false," +
                                    "    \"AllowMultipleIncomming\": false," +
                                    "    \"ReturnType\": \"boolean\"," +
                                    "    \"SQL\": \"CASE WHEN {0} IS NULL THEN {true} ELSE {false} END\"," +
                                    "    \"SQLBoolean\": \"{0} IS NULL\"," +
                                    "" +
                                    "    \"_ts\": 1530910445" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"IS NOT NULL Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Standard\"," +
                                    "    \"MinProperties\": 1," +
                                    "    \"MaxProperties\": 1," +
                                    "    \"Name\": \"IS NOT NULL\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-431d-933f-245c2cc2c2b9\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_ISNOTNULL\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"object\"," +
                                    "        \"integer\"," +
                                    "        \"number\"," +
                                    "        \"double\"," +
                                    "        \"float\"," +
                                    "        \"array\"," +
                                    "        \"string\"," +
                                    "        \"datetime\"" +
                                    "    ]," +
                                    "    \"AllowDifferentIncommingTypes\": false," +
                                    "    \"AllowMultipleIncomming\": false," +
                                    "    \"ReturnType\": \"boolean\"," +
                                    "    \"SQL\": \"CASE WHEN {0} IS NOT NULL THEN {true} ELSE {false} END\"," +
                                    "    \"SQLBoolean\": \"{0} IS NOT NULL\"," +
                                    "" +
                                    "    \"_ts\": 1530910449" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"Between Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Between\"," +
                                    "    \"MinProperties\": 1," +
                                    "    \"MaxProperties\": 1," +
                                    "    \"Name\": \"Between\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-521d-933f-743c2cc2c9c6\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_Between\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"number\"," +
                                    "        \"float\"," +
                                    "        \"integer\"," +
                                    "        \"double\"" +
                                    "    ]," +
                                    "    \"AllowDifferentIncommingTypes\": false," +
                                    "    \"AllowMultipleIncomming\": false," +
                                    "    \"ReturnType\": \"boolean\"," +
                                    "    \"SQL\": \"CASE WHEN {2} BETWEEN {0} AND {1} THEN {true} ELSE {false} END\"," +
                                    "    \"SQLBoolean\": \"{2} BETWEEN {0} AND {1}\"," +
                                    "" +
                                    "    \"_ts\": 1530910655" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"ToDateTime Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Standard\"," +
                                    "    \"MinProperties\": 1," +
                                    "    \"MaxProperties\": 1," +
                                    "    \"Name\": \"ToDateTime\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-431d-933f-343c2cc2c2b9\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_ToDateTime\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"string\"" +
                                    "    ]," +
                                    "    \"SQL\": \"TRY_CAST({0} AS datetime)\"," +
                                    "    \"AllowDifferentIncommingTypes\": false," +
                                    "    \"AllowMultipleIncomming\": false," +
                                    "    \"ReturnType\": \"datetime\"," +
                                    "" +
                                    "    \"_ts\": 1530910169" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"Average (Aggregate) Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Aggregate\"," +
                                    "    \"MinProperties\": 1," +
                                    "    \"MaxProperties\": 1," +
                                    "    \"Name\": \"Average\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-531d-933f-643c2cc2c9c7\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_AverageAggregate\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"integer\"," +
                                    "        \"double\"," +
                                    "        \"number\"," +
                                    "        \"float\"" +
                                    "    ]," +
                                    "    \"AllowDifferentIncommingTypes\": false," +
                                    "    \"AllowMultipleIncomming\": false," +
                                    "    \"ReturnType\": \"double\"," +
                                    "    \"SQL\": \"AVG({0})\"," +
                                    "" +
                                    "    \"_ts\": 1531501000" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"Min Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Aggregate\"," +
                                    "    \"MinProperties\": 1," +
                                    "    \"MaxProperties\": 1," +
                                    "    \"Name\": \"Min\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-531d-933f-643c2cc2c9c8\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_Min\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"integer\"," +
                                    "        \"double\"," +
                                    "        \"number\"," +
                                    "        \"float\"" +
                                    "    ]," +
                                    "    \"AllowDifferentIncommingTypes\": false," +
                                    "    \"AllowMultipleIncomming\": false," +
                                    "    \"ReturnType\": \"double\"," +
                                    "    \"SQL\": \"MIN({0})\"," +
                                    "" +
                                    "    \"_ts\": 1531169830" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"Max Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Aggregate\"," +
                                    "    \"MinProperties\": 1," +
                                    "    \"MaxProperties\": 1," +
                                    "    \"Name\": \"Max\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-531d-933f-643c2cc2c9e8\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_Max\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"integer\"," +
                                    "        \"double\"," +
                                    "        \"number\"," +
                                    "        \"float\"" +
                                    "    ]," +
                                    "    \"AllowDifferentIncommingTypes\": false," +
                                    "    \"AllowMultipleIncomming\": false," +
                                    "    \"ReturnType\": \"double\"," +
                                    "    \"SQL\": \"MAX({0})\"," +
                                    "" +
                                    "    \"_ts\": 1531169879" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"Sum (Aggregate) Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Aggregate\"," +
                                    "    \"MinProperties\": 1," +
                                    "    \"MaxProperties\": 1," +
                                    "    \"Name\": \"Sum\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-531d-933f-643c2cc2c9e7\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_SumAggregate\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"integer\"," +
                                    "        \"double\"," +
                                    "        \"number\"," +
                                    "        \"float\"" +
                                    "    ]," +
                                    "    \"AllowDifferentIncommingTypes\": false," +
                                    "    \"AllowMultipleIncomming\": false," +
                                    "    \"ReturnType\": \"double\"," +
                                    "    \"SQL\": \"SUM({0})\"," +
                                    "" +
                                    "    \"_ts\": 1531501009" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"Count Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"Aggregate\"," +
                                    "    \"MinProperties\": 1," +
                                    "    \"MaxProperties\": 1," +
                                    "    \"Name\": \"Count\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-531d-933f-643c2cc2c9e6\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_Count\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"boolean\"," +
                                    "        \"object\"," +
                                    "        \"null\"," +
                                    "        \"string\"," +
                                    "        \"integer\"," +
                                    "        \"number\"," +
                                    "        \"double\"," +
                                    "        \"float\"," +
                                    "        \"datetime\"" +
                                    "    ]," +
                                    "    \"AllowDifferentIncommingTypes\": false," +
                                    "    \"AllowMultipleIncomming\": false," +
                                    "    \"ReturnType\": \"double\"," +
                                    "    \"SQL\": \"COUNT({0})\"," +
                                    "" +
                                    "    \"_ts\": 1531509074" +
                                    "}," +
                                    "{" +
                                    "    \"AllowMany\": true," +
                                    "    \"Description\": \"Like Function\"," +
                                    "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                                    "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|SchemaFunctionDefinition\"," +
                                    "    \"FunctionType\": \"RegEx\"," +
                                    "    \"MinProperties\": 2," +
                                    "    \"MaxProperties\": 2," +
                                    "    \"Name\": \"Like\"," +
                                    "    \"Type\": \"SchemaFunctionDefinition\"," +
                                    "    \"id\": \"b3e56e96-80de-531d-933f-843c2cc2c9c8\"," +
                                    "    \"Active\": true," +
                                    "    \"Lookup\": \"Function_Like\"," +
                                    "    \"Created\": null," +
                                    "    \"Modified\": null," +
                                    "    \"AllowedIncommingTypes\": [" +
                                    "        \"string\"" +
                                    "    ]," +
                                    "    \"AllowDifferentIncommingTypes\": false," +
                                    "    \"AllowMultipleIncomming\": false," +
                                    "    \"ReturnType\": \"boolean\"," +
                                    "    \"SQL\": \"{1} LIKE {0}\"," +
                                    "" +
                                    "    \"_ts\": 1533014083" +
                                    "}]";

            return JsonConvert.DeserializeObject<List<SchemaFunctionDefinition>>(stringContent);
        }

        protected List<TypeDefinition> loadTypeDefinitionsFromJSON()
        {
            var stringContent = "[{" +
                "    \"AllowMany\": true," +
                "    \"Description\": \"Boolean Type\"," +
                "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|TypeDefinition\"," +
                "    \"ConversionMethod\": \"Function_ToBoolean\"," +
                "    \"Name\": \"Boolean Type\"," +
                "    \"Type\": \"TypeDefinition\"," +
                "    \"id\": \"b3e56e96-80de-431d-933f-243c2cc2c3c6\"," +
                "    \"Active\": true," +
                "    \"Lookup\": \"Type_Boolean\"," +
                "    \"Created\": null," +
                "    \"Modified\": null," +
                "    \"AllowedTypeNameConversions\": [" +
                "        \"boolean\"," +
                "        \"null\"," +
                "        \"string\"" +
                "    ]," +
                "    \"TypeName\": \"boolean\"," +
                "" +
                "    \"_ts\": 1513832849" +
                "}," +
                "{" +
                "    \"AllowMany\": true," +
                "    \"Description\": \"Integer Type\"," +
                "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|TypeDefinition\"," +
                "    \"ConversionMethod\": \"Function_ToInt\"," +
                "    \"Name\": \"Integer Type\"," +
                "    \"Type\": \"TypeDefinition\"," +
                "    \"id\": \"b3e56e96-80de-431d-933f-243c2cc2c3c9\"," +
                "    \"Active\": true," +
                "    \"Lookup\": \"Type_Integer\"," +
                "    \"Created\": null," +
                "    \"Modified\": null," +
                "    \"AllowedTypeNameConversions\": [" +
                "        \"string\"," +
                "        \"integer\"," +
                "        \"number\"," +
                "        \"double\"," +
                "        \"float\"" +
                "    ]," +
                "    \"TypeName\": \"integer\"," +
                "" +
                "    \"_ts\": 1513832871" +
                "}," +
                "{" +
                "    \"AllowMany\": true," +
                "    \"Description\": \"Number Type\"," +
                "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|TypeDefinition\"," +
                "    \"ConversionMethod\": \"Function_ToDouble\"," +
                "    \"Name\": \"Number Type\"," +
                "    \"Type\": \"TypeDefinition\"," +
                "    \"id\": \"b3e56e96-80de-431d-933f-243c2cc2c4c1\"," +
                "    \"Active\": true," +
                "    \"Lookup\": \"Type_Number\"," +
                "    \"Created\": null," +
                "    \"Modified\": null," +
                "    \"AllowedTypeNameConversions\": [" +
                "        \"string\"," +
                "        \"integer\"," +
                "        \"number\"," +
                "        \"double\"," +
                "        \"float\"" +
                "    ]," +
                "    \"TypeName\": \"number\"," +
                "" +
                "    \"_ts\": 1513832879" +
                "}," +
                "{" +
                "    \"AllowMany\": true," +
                "    \"Description\": \"Double Type\"," +
                "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|TypeDefinition\"," +
                "    \"ConversionMethod\": \"Function_ToDouble\"," +
                "    \"Name\": \"Double Type\"," +
                "    \"Type\": \"TypeDefinition\"," +
                "    \"id\": \"b3e56e96-80de-431d-933f-243c2cc2c4c2\"," +
                "    \"Active\": true," +
                "    \"Lookup\": \"Type_Double\"," +
                "    \"Created\": null," +
                "    \"Modified\": null," +
                "    \"AllowedTypeNameConversions\": [" +
                "        \"string\"," +
                "        \"integer\"," +
                "        \"number\"," +
                "        \"double\"," +
                "        \"float\"" +
                "    ]," +
                "    \"TypeName\": \"double\"," +
                "" +
                "    \"_ts\": 1513832890" +
                "}," +
                "{" +
                "    \"AllowMany\": true," +
                "    \"Description\": \"Float Type\"," +
                "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|TypeDefinition\"," +
                "    \"ConversionMethod\": \"Function_ToFloat\"," +
                "    \"Name\": \"Float Type\"," +
                "    \"Type\": \"TypeDefinition\"," +
                "    \"id\": \"b3e56e96-80de-431d-933f-243c2cc2c4c3\"," +
                "    \"Active\": true," +
                "    \"Lookup\": \"Type_Float\"," +
                "    \"Created\": null," +
                "    \"Modified\": null," +
                "    \"AllowedTypeNameConversions\": [" +
                "        \"string\"," +
                "        \"integer\"," +
                "        \"number\"," +
                "        \"double\"," +
                "        \"float\"" +
                "    ]," +
                "    \"TypeName\": \"float\"," +
                "" +
                "    \"_ts\": 1513832897" +
                "}," +
                "{" +
                "    \"AllowMany\": true," +
                "    \"Description\": \"DateTime Type\"," +
                "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|TypeDefinition\"," +
                "    \"ConversionMethod\": \"Function_ToDateTime\"," +
                "    \"Name\": \"DateTime Type\"," +
                "    \"Type\": \"TypeDefinition\"," +
                "    \"id\": \"b3e56e96-80de-431d-933f-243c2cc2c4c8\"," +
                "    \"Active\": true," +
                "    \"Lookup\": \"Type_DateTime\"," +
                "    \"Created\": null," +
                "    \"Modified\": null," +
                "    \"AllowedTypeNameConversions\": [" +
                "        \"string\"," +
                "        \"datetime\"" +
                "    ]," +
                "    \"TypeName\": \"datetime\"," +
                "" +
                "    \"_ts\": 1531790240" +
                "}," +
                "{" +
                "    \"AllowMany\": true," +
                "    \"Description\": \"String Type\"," +
                "    \"EnterpriseID\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408\"," +
                "    \"EnterpriseTypeKey\": \"51eb2185-dd6c-47a3-bec0-19da4bd6a408|TypeDefinition\"," +
                "    \"Name\": \"String Type\"," +
                "    \"Type\": \"TypeDefinition\"," +
                "    \"id\": \"b3e56e96-80de-431d-933f-243c2cc2c3c5\"," +
                "    \"Active\": true," +
                "    \"Lookup\": \"Type_String\"," +
                "    \"Created\": null," +
                "    \"Modified\": null," +
                "    \"AllowedTypeNameConversions\": [" +
                "        \"boolean\"," +
                "        \"null\"," +
                "        \"integer\"," +
                "        \"number\"," +
                "        \"double\"," +
                "        \"float\"," +
                "        \"string\"" +
                "    ]," +
                "    \"TypeName\": \"string\"," +
                "" +
                "    \"_ts\": 1541037982" +
                "}]";

            return JsonConvert.DeserializeObject<List<TypeDefinition>>(stringContent);
        }
        #endregion
    }
}

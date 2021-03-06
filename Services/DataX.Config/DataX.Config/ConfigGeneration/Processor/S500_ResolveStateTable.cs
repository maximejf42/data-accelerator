﻿// *********************************************************************
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License
// *********************************************************************
using Newtonsoft.Json;
using DataX.Config.ConfigDataModel.RuntimeConfig;
using DataX.Contract;
using DataX.Flow.CodegenRules;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataX.Config.ConfigGeneration.Processor
{
    /// <summary>
    /// Produce the accumulation/state tables section
    /// </summary>
    [Shared]
    [Export(typeof(IFlowDeploymentProcessor))]
    public class ResolveStateTable: ProcessorBase
    {
        public const string TokenName_StateTables = "processStateTables";
        
        public override async Task<string> Process(FlowDeploymentSession flowToDeploy)
        {
            var config = flowToDeploy.Config;
            var guiConfig = config?.GetGuiConfig();
            if (guiConfig == null)
            {
                return "no gui input, skipped.";
            }

            var rulesCode = flowToDeploy.GetAttachment<RulesCode>(GenerateTransformFile.AttachmentName_CodeGenObject);
            Ensure.NotNull(rulesCode, "rulesCode");
            Ensure.NotNull(rulesCode.MetricsRoot, "rulesCode.MetricsRoot");
            Ensure.NotNull(rulesCode.MetricsRoot.metrics, "rulesCode.MetricsRoot.metrics");

            var stateTables = rulesCode.AccumlationTables.Select(t =>
            {
                return new StateTableSpec()
                {
                    Name = t.Key,
                    Schema = t.Value,
                    Location = ConstructStateTablePath(guiConfig.Name, t.Key)
            };
            }).ToArray();

            flowToDeploy.SetObjectToken(TokenName_StateTables, stateTables);

            await Task.Yield();

            return "done";
        }


        private string ConstructStateTablePath(string flowName, string tableName)
        {
            return $"hdfs://mycluster/datax/{flowName}/{tableName}/";
        }
    }
}

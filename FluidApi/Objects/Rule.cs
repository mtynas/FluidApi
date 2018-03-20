using System;
using System.Collections.Generic;


namespace FluidAutomationService.Objects
{
    public class Rule
    {
       
        public string displayName       {get; set;}
        public string ruleDescription   {get; set;}
        public string ruleScope         {get; set;}

        public Int64 uid {get; set;}
        public DateTime lastTriggerDate;

        public List <RuleCriteria> criterias {get; set;}

       
    }
}

﻿using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

namespace RvtRigging
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CmdPinLinesets : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            UIApplication uiapp = commandData.Application;
            Document doc = uidoc.Document;

            if (doc.IsFamilyDocument)
            {
                TaskDialog.Show("Document Error", "Command only available in Model");
                return Result.Failed;
            }


            //Create Filtered Element Collector & Filter for Linesets
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ElementQuickFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_SpecialityEquipment);

            //Apply Filter
            IList<Element> eles = collector.WherePasses(filter).WhereElementIsNotElementType().ToElements();

            //Locate all existing the Lineset Families  
            IList<Lineset> linesets = new List<Lineset>();
            foreach (Element ele in eles)
            {
                ElementId typeId = ele.GetTypeId();
                ElementType type = doc.GetElement(typeId) as ElementType;
                if (type.get_Parameter(TCCRiggingSettings.IsLinesetGuid) != null)
                    if(type.get_Parameter(TCCRiggingSettings.IsLinesetGuid).AsInteger() == 1)
                        linesets.Add(new Lineset(doc, ele));
            }


            using (Transaction trans = new Transaction(doc, "Pin Lineset"))
            {
                trans.Start();

                foreach(Lineset ls in linesets)
                {
                    ls.lsElement.Pinned = true;
                }

                trans.Commit();
            }

            return Result.Succeeded;
        }
    }
}

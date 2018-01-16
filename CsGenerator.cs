using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace DataTierGenerator
{
	/// <summary>
	/// Generates C# data access and data transfer classes.
	/// </summary>
	internal static class CsGenerator
	{
		/// <summary>
		/// Creates a project file that references each generated C# code file for data access.
		/// </summary>
		/// <param name="path">The path where the project file should be created.</param>
		/// <param name="projectName">The name of the project.</param>
		/// <param name="tableList">The list of tables code files were created for.</param>
		/// <param name="daoSuffix">The suffix to append to the name of each data access class.</param>
        /// <param name="dtoSuffix">The suffix to append to the name of each data transfer class.</param>
		public static void CreateProjectFile(string path, string projectName, List<Table> tableList, string daoSuffix, string dtoSuffix)
		{
			string projectXml = Utility.GetResource("DataTierGenerator.Resources.Project.xml");
			XmlDocument document = new XmlDocument();
			document.LoadXml(projectXml);

			XmlNamespaceManager namespaceManager = new XmlNamespaceManager(document.NameTable);
			namespaceManager.AddNamespace(String.Empty, "http://schemas.microsoft.com/developer/msbuild/2003");
			namespaceManager.AddNamespace("msbuild", "http://schemas.microsoft.com/developer/msbuild/2003");

			document.SelectSingleNode("/msbuild:Project/msbuild:PropertyGroup/msbuild:ProjectGuid", namespaceManager).InnerText = "{" + Guid.NewGuid().ToString() + "}";
			document.SelectSingleNode("/msbuild:Project/msbuild:PropertyGroup/msbuild:RootNamespace", namespaceManager).InnerText = projectName;
			document.SelectSingleNode("/msbuild:Project/msbuild:PropertyGroup/msbuild:AssemblyName", namespaceManager).InnerText = projectName;

			XmlNode itemGroupNode = document.SelectSingleNode("/msbuild:Project/msbuild:ItemGroup[msbuild:Compile]", namespaceManager);
			foreach (Table table in tableList)
			{
				string className = Utility.FormatClassName(table.Name);
				
				XmlNode dtoCompileNode = document.CreateElement("Compile", "http://schemas.microsoft.com/developer/msbuild/2003");
				XmlAttribute dtoAttribute = document.CreateAttribute("Include");
				dtoAttribute.Value = className + dtoSuffix + ".cs";
				dtoCompileNode.Attributes.Append(dtoAttribute);
				itemGroupNode.AppendChild(dtoCompileNode);
				
				XmlNode dataCompileNode = document.CreateElement("Compile", "http://schemas.microsoft.com/developer/msbuild/2003");
				XmlAttribute dataAttribute = document.CreateAttribute("Include");
				dataAttribute.Value = Path.Combine("Repositories", Utility.FormatClassName(table.Name) + daoSuffix + ".cs");
				dataCompileNode.Attributes.Append(dataAttribute);
				itemGroupNode.AppendChild(dataCompileNode);
			}
			
			document.Save(Path.Combine(path, projectName + ".csproj"));
		}

		/// <summary>
		/// Creates the AssemblyInfo.cs file for the project.
		/// </summary>
		/// <param name="path">The root path of the project.</param>
		/// <param name="assemblyTitle">The title of the assembly.</param>
		/// <param name="databaseName">The name of the database the assembly provides access to.</param>
		public static void CreateAssemblyInfo(string path, string assemblyTitle, string databaseName)
		{
			string assemblyInfo = Utility.GetResource("DataTierGenerator.Resources.AssemblyInfo.txt");
			assemblyInfo.Replace("#AssemblyTitle", assemblyTitle);
			assemblyInfo.Replace("#DatabaseName", databaseName);

			string propertiesDirectory = Path.Combine(path, "Properties");
			if (Directory.Exists(propertiesDirectory) == false)
			{
				Directory.CreateDirectory(propertiesDirectory);
			}

			File.WriteAllText(Path.Combine(propertiesDirectory, "AssemblyInfo.cs"), assemblyInfo);
		}

		/// <summary>
		/// Creates the SharpCore DLLs required by the generated code.
		/// </summary>
		/// <param name="path">The root path of the project</param>
		public static void CreateSharpCore(string path)
		{
			string sharpCoreDirectory = Path.Combine(Path.Combine(path, "Lib"), "SharpCore");
			if (Directory.Exists(sharpCoreDirectory) == false)
			{
				Directory.CreateDirectory(sharpCoreDirectory);
			}

			Utility.WriteResourceToFile("DataTierGenerator.Resources.SharpCore.SharpCore.Data.dll", Path.Combine(sharpCoreDirectory, "SharpCore.Data.dll"));
			Utility.WriteResourceToFile("DataTierGenerator.Resources.SharpCore.SharpCore.Data.pdb", Path.Combine(sharpCoreDirectory, "SharpCore.Data.pdb"));
			Utility.WriteResourceToFile("DataTierGenerator.Resources.SharpCore.SharpCore.Extensions.dll", Path.Combine(sharpCoreDirectory, "SharpCore.Extensions.dll"));
			Utility.WriteResourceToFile("DataTierGenerator.Resources.SharpCore.SharpCore.Extensions.pdb", Path.Combine(sharpCoreDirectory, "SharpCore.Extensions.pdb"));
			Utility.WriteResourceToFile("DataTierGenerator.Resources.SharpCore.SharpCore.Utilities.dll", Path.Combine(sharpCoreDirectory, "SharpCore.Utilities.dll"));
			Utility.WriteResourceToFile("DataTierGenerator.Resources.SharpCore.SharpCore.Utilities.pdb", Path.Combine(sharpCoreDirectory, "SharpCore.Utilities.pdb"));
		}

		/// <summary>
		/// Creates a C# class for all of the table's stored procedures.
		/// </summary>
		/// <param name="table">Instance of the Table class that represents the table this class will be created for.</param>
		/// <param name="targetNamespace">The namespace that the generated C# classes should contained in.</param>
		/// <param name="daoSuffix">The suffix to be appended to the data access class.</param>
		/// <param name="path">Path where the class should be created.</param>
		public static void CreateDataTransferClass(Table table, string targetNamespace, string dtoSuffix, string path)
		{
            //string className = Utility.FormatClassName(table.Name) + dtoSuffix;
            string className = table.Name;  //Utility.FormatClassName(table.Name) + dtoSuffix;

            using (StreamWriter streamWriter = new StreamWriter(Path.Combine(path, className + ".cs")))
			{
				// Create the header for the class
				streamWriter.WriteLine("using System;");
				streamWriter.WriteLine();
				streamWriter.WriteLine("namespace " + targetNamespace);
				streamWriter.WriteLine("{");

				streamWriter.WriteLine("\tpublic class " + className+ ":EntityBase");
				streamWriter.WriteLine("\t{");

				// Create an explicit public constructor
				streamWriter.WriteLine("\t\t#region Constructors");
				streamWriter.WriteLine();
				streamWriter.WriteLine("\t\t/// <summary>");
				streamWriter.WriteLine("\t\t/// Initializes a new instance of the " + className + " class.");
				streamWriter.WriteLine("\t\t/// </summary>");
				streamWriter.WriteLine("\t\tpublic " + className + "()");
				streamWriter.WriteLine("\t\t{");
				streamWriter.WriteLine("\t\t}");
				streamWriter.WriteLine();

				// Create the "partial" constructor
				int parameterCount = 0;
				streamWriter.WriteLine("\t\t/// <summary>");
				streamWriter.WriteLine("\t\t/// Initializes a new instance of the " + className + " class.");
				streamWriter.WriteLine("\t\t/// </summary>");
				streamWriter.Write("\t\tpublic " + className + "(");
				for (int i = 0; i < table.Columns.Count; i++)
				{
					Column column = table.Columns[i];
					if (column.IsIdentity == false && column.IsRowGuidCol == false)
					{
						streamWriter.Write(Utility.CreateMethodParameter(column));
						if (i < (table.Columns.Count - 1))
						{
							streamWriter.Write(", ");
						}
						parameterCount++;
					}
				}
				streamWriter.WriteLine(")");
				streamWriter.WriteLine("\t\t{");
				foreach (Column column in table.Columns)
				{
					if (column.IsIdentity == false && column.IsRowGuidCol == false)
					{
						streamWriter.WriteLine("\t\t\tthis." + Utility.FormatPascal(column.Name) + " = " + Utility.FormatCamel(column.Name) + ";");
					}
				}
				streamWriter.WriteLine("\t\t}");

				// Create the "full featured" constructor, if we haven't already
				if (parameterCount < table.Columns.Count)
				{
					streamWriter.WriteLine();
					streamWriter.WriteLine("\t\t/// <summary>");
					streamWriter.WriteLine("\t\t/// Initializes a new instance of the " + className + " class.");
					streamWriter.WriteLine("\t\t/// </summary>");
					streamWriter.Write("\t\tpublic " + className + "(");
					for (int i = 0; i < table.Columns.Count; i++)
					{
						Column column = table.Columns[i];
						streamWriter.Write(Utility.CreateMethodParameter(column));
						if (i < (table.Columns.Count - 1))
						{
							streamWriter.Write(", ");
						}
					}
					streamWriter.WriteLine(")");
					streamWriter.WriteLine("\t\t{");
					foreach (Column column in table.Columns)
					{
						streamWriter.WriteLine("\t\t\tthis." + Utility.FormatPascal(column.Name) + " = " + Utility.FormatCamel(column.Name) + ";");
					}
					streamWriter.WriteLine("\t\t}");
				}

				streamWriter.WriteLine();
				streamWriter.WriteLine("\t\t#endregion");
				streamWriter.WriteLine();

				// Append the public properties
				streamWriter.WriteLine("\t\t#region Properties");
				for (int i = 0; i < table.Columns.Count; i++)
				{
					Column column = table.Columns[i];
					string parameter = Utility.CreateMethodParameter(column);
					string type = parameter.Split(' ')[0];
					string name = parameter.Split(' ')[1];

					streamWriter.WriteLine("\t\t/// <summary>");
					streamWriter.WriteLine("\t\t/// Gets or sets the " + Utility.FormatPascal(name) + " value.");
					streamWriter.WriteLine("\t\t/// </summary>");
					streamWriter.WriteLine("\t\tpublic " + type + " " + Utility.FormatPascal(name) + " { get; set; }");

					if (i < (table.Columns.Count - 1))
					{
						streamWriter.WriteLine();
					}
				}
				
				streamWriter.WriteLine();
				streamWriter.WriteLine("\t\t#endregion");

				// Close out the class and namespace
				streamWriter.WriteLine("\t}");
				streamWriter.WriteLine("}");
			}
		}
		
		/// <summary>
		/// Creates a C# data access class for all of the table's stored procedures.
		/// </summary>
		/// <param name="databaseName">The name of the database.</param>
		/// <param name="table">Instance of the Table class that represents the table this class will be created for.</param>
		/// <param name="targetNamespace">The namespace that the generated C# classes should contained in.</param>
		/// <param name="storedProcedurePrefix">Prefix to be appended to the name of the stored procedure.</param>
		/// <param name="daoSuffix">The suffix to be appended to the data access class.</param>
        /// <param name="dtoSuffix">The suffix to append to the name of each data transfer class.</param>
		/// <param name="path">Path where the class should be created.</param>
		public static void CreateDataAccessClass(string databaseName, Table table, string targetNamespace, string storedProcedurePrefix, string daoSuffix, string dtoSuffix, string path)
		{
			string className = Utility.FormatClassName(table.Name) + daoSuffix;
			path = Path.Combine(path, "Repositories");
			
			using (StreamWriter streamWriter = new StreamWriter(Path.Combine(path, className + ".cs")))
			{
				// Create the header for the class
				streamWriter.WriteLine("using System;");
				streamWriter.WriteLine("using System.Collections.Generic;");
				streamWriter.WriteLine("using System.Data;");
				streamWriter.WriteLine("using System.Data.SqlClient;");
                streamWriter.WriteLine("using smartOffice_Models;");
                streamWriter.WriteLine("using System.Configuration;");


				streamWriter.WriteLine();

				streamWriter.WriteLine("namespace " + targetNamespace );
				streamWriter.WriteLine("{");

				streamWriter.WriteLine("\tpublic class " + className + "DL");
				streamWriter.WriteLine("\t{");
                streamWriter.WriteLine("\t");
                streamWriter.WriteLine("\t string strquery = \"\";");
                // 
				// Append the fields
				streamWriter.WriteLine("\t\t#region Fields");
				streamWriter.WriteLine();
				streamWriter.WriteLine("\t\tprivate string connectionStringName;");
				streamWriter.WriteLine();
				streamWriter.WriteLine("\t\t#endregion");
				streamWriter.WriteLine();
				
			    // Append the access methods
				streamWriter.WriteLine("\t\t#region Methods");
				streamWriter.WriteLine();
				
				CreateInsertMethod(table, storedProcedurePrefix, dtoSuffix, streamWriter);
                
          
				streamWriter.WriteLine();
				streamWriter.WriteLine("\t\t#endregion");

				// Close out the class and namespace
				streamWriter.WriteLine("\t}");
				streamWriter.WriteLine("}");
			}
		}

		/// <summary>
		/// Creates a string that represents the insert functionality of the data access class.
		/// </summary>
		/// <param name="table">The Table instance that this method will be created for.</param>
		/// <param name="storedProcedurePrefix">The prefix that is used on the stored procedure that this method will call.</param>
        /// <param name="dtoSuffix">The suffix to append to the name of each data transfer class.</param>
		/// <param name="streamWriter">The StreamWriter instance that will be used to create the method.</param>
		private static void CreateInsertMethod(Table table, string storedProcedurePrefix, string dtoSuffix, StreamWriter streamWriter)
		{
			string className = Utility.FormatClassName(table.Name) + dtoSuffix;
			string variableName = Utility.FormatVariableName(table.Name);

			// Append the method header
			streamWriter.WriteLine("\t\t/// <summary>");
			streamWriter.WriteLine("\t\t/// Saves a record to the " + table.Name + " table.");
			streamWriter.WriteLine("\t\t/// </summary>");
            streamWriter.WriteLine("\t\tpublic Boolean Save" + variableName + "SP(" + variableName + " " + variableName + ", int formMode)");
			streamWriter.WriteLine("\t\t{");
			
			// Append validation for the parameter

            streamWriter.WriteLine("\t\t\tSqlCommand scom;");
            streamWriter.WriteLine("\t\t\tbool retvalue = false;");
            streamWriter.WriteLine("\t\t\ttry");
            streamWriter.WriteLine("\t\t\t{");
            streamWriter.WriteLine("\t\t\tscom = new SqlCommand();");
            streamWriter.WriteLine("\t\t\tscom.CommandType = CommandType.StoredProcedure;");
            streamWriter.WriteLine("\t\t\tscom.CommandText = \"" + className.Trim() + "Save\";");
			streamWriter.WriteLine("");
			
			// Append the parameter declarations
			for (int i = 0; i < table.Columns.Count; i++)
			{
				Column column = table.Columns[i];
				if (column.IsIdentity == false && column.IsRowGuidCol == false)
				{
                    streamWriter.Write("\t\t\t\t" + "scom.Parameters.Add(\"@" + column.Name + "\", SqlDbType." + Utility.GetSqlDbType(column.Type.Trim()) + ", " + column.Length.ToString() + ").Value = " + variableName + "." + column.Name + ";");
					streamWriter.WriteLine();
				}
			}

            streamWriter.WriteLine("\t\t\t scom.Parameters.Add(\"@InsMode\", SqlDbType.Int).Value = formMode; // For insert");
            streamWriter.WriteLine("\t\t\t scom.Parameters.Add(\"@RtnValue\", SqlDbType.Int).Value = 0;");

            streamWriter.WriteLine("");
            streamWriter.WriteLine("\t\t\tu_DBConnection dbcon = new u_DBConnection();");
            streamWriter.WriteLine("\t\t\tretvalue = dbcon.RunQuery(scom);");
            streamWriter.WriteLine("\t\t\treturn retvalue;");
            streamWriter.WriteLine("\t\t\t}");
            streamWriter.WriteLine("\t\t\tcatch (Exception ex)");
            streamWriter.WriteLine("\t\t\t{");
            streamWriter.WriteLine("\t\t\tthrow (ex);");
            streamWriter.WriteLine("\t\t\t}");
            streamWriter.WriteLine("\t\t}");


            streamWriter.WriteLine("\t\t\t");
            streamWriter.WriteLine("\t\t\t ");

            streamWriter.WriteLine("\t\t\t  public DataTable SelectAll" + variableName + "()");
            streamWriter.WriteLine("\t\t {");
            streamWriter.WriteLine("\t\t\t try");
            streamWriter.WriteLine("\t\t {");
            streamWriter.WriteLine("\t\t\t strquery = @\"select [CompCode],	[Descr] from ["+ className +"]\";");
            streamWriter.WriteLine("\t\t\t DataTable dt"+variableName +" = u_DBConnection.ReturnDataTable(strquery, CommandType.Text);");
            streamWriter.WriteLine("\t\t\t return dt" + variableName  + ";");
            streamWriter.WriteLine("\t\t\t }");
            streamWriter.WriteLine("\t\t\t catch (Exception ex)");
            streamWriter.WriteLine("\t\t\t {");
            streamWriter.WriteLine("\t\t\t  throw ex;");
            streamWriter.WriteLine("\t\t\t }");
            streamWriter.WriteLine("\t\t }");
            streamWriter.WriteLine("\t\t\t ");
            streamWriter.WriteLine("\t\t\t ");





            streamWriter.WriteLine("\t\t\t public " + variableName + " Select" + variableName + "(" + variableName + " obj" + variableName + ")");
            streamWriter.WriteLine("\t\t\t {");
            streamWriter.WriteLine("\t\t\t try");
            streamWriter.WriteLine("\t\t\t {");
            //strquery = @"select * from M_Company where CompCode = '"  + objm_Company + "'" ;
            streamWriter.WriteLine("\t\t\t strquery = @\"select * from " + variableName + " where CompCode = '\" + " + "obj" + variableName + " + \"'\";");
            streamWriter.WriteLine("\t\t\t DataRow drType = u_DBConnection.ReturnDataRow(strquery);");
            streamWriter.WriteLine("\t\t\t if (drType != null)");
            streamWriter.WriteLine("\t\t\t {");

            // Append the parameter declarations
            for (int i = 0; i < table.Columns.Count; i++)
            {
                Column column = table.Columns[i];
                if (column.IsIdentity == false && column.IsRowGuidCol == false)
                {

                    streamWriter.WriteLine("\t\t\t\t obj" + variableName + "." + column.Name.Trim() + " = " + Utility.GetConvertionType(column.Type).Trim() + " drType[\"" + column.Name.Trim() + "\"].ToString()" + Utility.GetConvertionTypeEnds(column.Type).Trim() + ";");
                }
            }


            streamWriter.WriteLine("\t\t\t return obj" + variableName.Trim() + ";");
            streamWriter.WriteLine("\t\t\t }");
            streamWriter.WriteLine("\t\t\t return null;");
            streamWriter.WriteLine("\t\t\t }");
            streamWriter.WriteLine("\t\t\t catch (Exception ex)");
            streamWriter.WriteLine("\t\t\t {");
            streamWriter.WriteLine("\t\t\t throw ex;");
            streamWriter.WriteLine("\t\t\t }");
            streamWriter.WriteLine("\t\t\t }");
            streamWriter.WriteLine("\t\t\t ");




            streamWriter.WriteLine("\t\t\t public static bool Existing"+ className+"(string string"+variableName+") ");
            streamWriter.WriteLine("\t\t\t {");
            streamWriter.WriteLine("\t\t\t try");
            streamWriter.WriteLine("\t\t\t {");
            streamWriter.WriteLine("\t\t\t string xstrquery = @\"select CompCode From " + className + "   WHERE CompCode = '\"+ string" + variableName + "+ \"' \";");
            streamWriter.WriteLine("\t\t\t DataRow dr"+className+" = u_DBConnection.ReturnDataRow(xstrquery);");
            streamWriter.WriteLine("\t\t\t if (dr" +className+" != null)");
            streamWriter.WriteLine("\t\t\t {");
            streamWriter.WriteLine("\t\t\t return true;");
            streamWriter.WriteLine("\t\t\t }");
            streamWriter.WriteLine("\t\t\t return false;");
            streamWriter.WriteLine("\t\t\t }");
            streamWriter.WriteLine("\t\t\t catch (Exception ex)");
            streamWriter.WriteLine("\t\t\t {");
            streamWriter.WriteLine("\t\t\t throw ex;");
            streamWriter.WriteLine("\t\t\t  }");
            streamWriter.WriteLine("\t\t  }");
            streamWriter.WriteLine("\t\t\t  ");

            streamWriter.WriteLine("\t\t\t  public List<" + variableName.Trim() + "> Select" + className.Trim() + "Multi(" + variableName.Trim() + " obj" + variableName.Trim() + "2)");
            streamWriter.WriteLine("\t\t\t {");
            streamWriter.WriteLine("\t\t\t List<" + variableName.Trim() + "> retval = new List<" + variableName.Trim() + ">();");
            streamWriter.WriteLine("\t\t\t try");
            streamWriter.WriteLine("\t\t\t {");
            streamWriter.WriteLine("\t\t\t strquery = @\"select * from " + variableName.Trim() + " where purchaseReqNo = '\" + obj" + variableName.Trim() + "2.xxxx + \"'\";");
            streamWriter.WriteLine("\t\t\t DataTable dt"+variableName.Trim()+" = u_DBConnection.ReturnDataTable(strquery, CommandType.Text);");
            streamWriter.WriteLine("\t\t\t foreach (DataRow drType in dt" + variableName.Trim() + ".Rows)");
            streamWriter.WriteLine("\t\t\t  {");
            streamWriter.WriteLine("\t\t\t if (drType != null)");
            streamWriter.WriteLine("\t\t\t {");
            streamWriter.WriteLine("\t\t\t " + variableName.Trim() + " obj" + variableName.Trim() + " = new " + variableName.Trim() + "();");

            // Append the parameter declarations
            for (int i = 0; i < table.Columns.Count; i++)
            {
                Column column = table.Columns[i];
                if (column.IsIdentity == false && column.IsRowGuidCol == false)
                {
                   streamWriter.WriteLine("\t\t\t\t obj" + variableName + "." + column.Name.Trim() + " = " + Utility.GetConvertionType(column.Type).Trim() + " drType[\"" + column.Name.Trim() + "\"].ToString()" + Utility.GetConvertionTypeEnds(column.Type).Trim() + ";");
                }
            }

            streamWriter.WriteLine("\t\t\t retval.Add(obj" + variableName + ");");
            streamWriter.WriteLine("\t\t\t }");
            streamWriter.WriteLine("\t\t\t }");
            streamWriter.WriteLine("\t\t\t return retval;");
            streamWriter.WriteLine("\t\t\t }");
            streamWriter.WriteLine("\t\t\t catch (Exception ex)");
            streamWriter.WriteLine("\t\t\t {");
            streamWriter.WriteLine("\t\t\t throw ex;");
            streamWriter.WriteLine("\t\t\t }");
            streamWriter.WriteLine("\t\t\t }");
            streamWriter.WriteLine("\t\t\t ");
            streamWriter.WriteLine("\t\t\t ");

			// Append the method footer
			streamWriter.WriteLine("\t\t");
			streamWriter.WriteLine();
		}







        /// <summary>
        /// Creates a string that represents the insert functionality of the data access class.
        /// </summary>
        /// <param name="table">The Table instance that this method will be created for.</param>
        /// <param name="storedProcedurePrefix">The prefix that is used on the stored procedure that this method will call.</param>
        /// <param name="dtoSuffix">The suffix to append to the name of each data transfer class.</param>
        /// <param name="streamWriter">The StreamWriter instance that will be used to create the method.</param>
        private static void CreateFormMethods(Table table, string storedProcedurePrefix, string dtoSuffix, StreamWriter streamWriter)
        {
            string className = Utility.FormatClassName(table.Name) + dtoSuffix;
            string variableName = Utility.FormatVariableName(table.Name);

            streamWriter.WriteLine("\t\t\t #region  SetValues");
            streamWriter.WriteLine("\t\t\t private void SetValues(String s" + variableName.Trim() + ") ");
            streamWriter.WriteLine("\t\t\t { ");
            streamWriter.WriteLine("\t\t\t   try");
            streamWriter.WriteLine("\t\t\t  {");
            streamWriter.WriteLine("\t\t\t  " + className + "DL obj" + variableName + "DL = new " + className + "DL();");
            streamWriter.WriteLine("\t\t\t  " + className + " obj" + variableName + " = new " + className + "();");
            streamWriter.WriteLine("\t\t\t  if (s"+variableName +" != \"\")");
            streamWriter.WriteLine("\t\t\t  {");
            streamWriter.WriteLine("\t\t\t  obj"+variableName+".CompCode = s"+variableName+";");
            streamWriter.WriteLine("\t\t\t  obj" + variableName + " = obj" + variableName + "DL.Select"+variableName+"(obj"+variableName+");");
            streamWriter.WriteLine("\t\t\t   if (obj" + variableName + " != null)");
            streamWriter.WriteLine("\t\t\t  {");
            
        
            // Append the parameter declarations
            for (int i = 0; i < table.Columns.Count; i++)
            {
                Column column = table.Columns[i];
                if (column.IsIdentity == false && column.IsRowGuidCol == false)
                {
                    // txt_comcode.Text = objBank.CompCode.ToString();
                    streamWriter.WriteLine("\t\t\t\t   txt_" + column.Name + ".Text = obj" + variableName + "." + column.Name + ".ToString();");
                }
            }


            streamWriter.WriteLine("\t\t\t  formMode = 3;");
            streamWriter.WriteLine("\t\t\t  }");
            streamWriter.WriteLine("\t\t\t  }");
            streamWriter.WriteLine("\t\t\t   }");
            streamWriter.WriteLine("\t\t\t  catch (Exception ex)");
            streamWriter.WriteLine("\t\t\t  {");
            streamWriter.WriteLine("\t\t\t  throw ex;");
            streamWriter.WriteLine("\t\t\t  }");
            streamWriter.WriteLine("\t\t\t  }");
            streamWriter.WriteLine("\t\t\t  #endregion ");
            streamWriter.WriteLine("\t\t\t  ");
            streamWriter.WriteLine("\t\t\t  ");





            //
            //
            //GetData();


            streamWriter.WriteLine("\t\t\t " + className + " obj" + variableName + " = new " + className + "();  ");
            // Append the parameter declarations
            for (int i = 0; i < table.Columns.Count; i++)
            {
                Column column = table.Columns[i];
                if (column.IsIdentity == false && column.IsRowGuidCol == false)
                {
                    //objbank.CompCode = txt_comcode.Text.Trim();
                    streamWriter.WriteLine("\t\t\t\t   obj" + variableName + "." + column.Name.Trim() + " = txt_" + column.Name.Trim() + ".Text.Trim();");
                }
            }

            streamWriter.WriteLine("\t\t\t " + className + "DL bal = new " + className + "DL();  ");
            streamWriter.WriteLine("\t\t\t  bal.Save"+className+"SP(obj"+variableName+", 1);");
            streamWriter.WriteLine("\t\t\t  "); //SaveM_CompanySP
         



            // Append the method footer
            streamWriter.WriteLine("\t\t");
            streamWriter.WriteLine();
        }


        public static void CreateFORMClasses(string databaseName, Table table, string targetNamespace, string storedProcedurePrefix, string daoSuffix, string dtoSuffix, string path)
        {
            string className = Utility.FormatClassName(table.Name) + daoSuffix;
            path = Path.Combine(path, "FormClasses");

            using (StreamWriter streamWriter = new StreamWriter(Path.Combine(path, className + ".cs")))
            {

                CreateFormMethods(table, storedProcedurePrefix, dtoSuffix, streamWriter);

            }
        }


        public static void DapperClasses(string databaseName, Table table, string targetNamespace, string storedProcedurePrefix, string daoSuffix, string dtoSuffix, string path)
        {
            string className = Utility.FormatClassName(table.Name) + daoSuffix;
            path = Path.Combine(path, "Dapper");

            using (StreamWriter streamWriter = new StreamWriter(Path.Combine(path, className + ".cs")))
            {

                CreateDapper(table, storedProcedurePrefix, dtoSuffix, streamWriter);

            }
        }



        /*
          public override InvHed Save2(InvHed entity)
        {
            try
            {
                using (var scope = new TransactionScope())
                {

                    var param = new DynamicParameters();
                    param.Add("@ID", value: entity.ID, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
                    param.Add("@DocNo", value: entity.DocNo);
                    param.Add("@Gross", value: entity.Gross);
                    param.Add("@NetValue", value: entity.NetValue);

                    Connection.Execute("SaveInvHead", param, commandType: CommandType.StoredProcedure);
                    entity.ID = param.Get<int>("@ID");

                    scope.Complete();
                }
                return entity;
            }
            catch (InvalidOperationException ioe)
            {
                throw new Exception(ErrorRepository.GetErrorMessage(ErrorNumbers.E1002));
            }
            catch (SqlException ex)
            {
                throw new Exception(ErrorRepository.GetErrorMessage(ErrorNumbers.E1001));
            }
            catch (Exception ex)
            {
                throw new Exception(ErrorRepository.GetErrorMessage(ErrorNumbers.E1000));
            }
        }
         */

        private static void CreateDapper(Table table, string storedProcedurePrefix, string dtoSuffix, StreamWriter streamWriter)
        {
            string className = Utility.FormatClassName(table.Name) + dtoSuffix;
            string variableName = Utility.FormatVariableName(table.Name);

            streamWriter.WriteLine("public override " + className + " Save2(" + className + " entity) ");
            streamWriter.WriteLine("{");
            streamWriter.WriteLine("\t  try");
            streamWriter.WriteLine("\t  {  ");
            streamWriter.WriteLine("\t\t  using (var scope = new TransactionScope())  ");
            streamWriter.WriteLine("\t\t  { ");
            streamWriter.WriteLine("\t\t  var param = new DynamicParameters();");
         

            // Append the parameter declarations
            for (int i = 0; i < table.Columns.Count; i++)
            {
                Column column = table.Columns[i];
                string str = "\"@" + column.Name + "\"";
                if (column.IsIdentity == false && column.IsRowGuidCol == false)
                {
                    streamWriter.WriteLine("\t\t  param.Add(" + str + ", value: entity.DocNo);  ");
                }
                else
                {
                    streamWriter.WriteLine("\t\t  param.Add( " + str + " , value: entity." + column.Name + ", dbType: DbType.Int32, direction: ParameterDirection.InputOutput)");
                }
            }

            /*
                    Connection.Execute("SaveInvHead", param, commandType: CommandType.StoredProcedure);
                    entity.ID = param.Get<int>("@ID");

                    scope.Complete();
                }
                return entity;
            */

            
            streamWriter.WriteLine("\t\t   ");
            streamWriter.WriteLine("\t\t   ");
            streamWriter.WriteLine("\t\t   ");
            streamWriter.WriteLine("\t\t   ");
            streamWriter.WriteLine("\t\t   ");
            streamWriter.WriteLine("\t\t   ");



            streamWriter.WriteLine("\t\t\t " + className + " obj" + variableName + " = new " + className + "();  ");
            // Append the parameter declarations
            for (int i = 0; i < table.Columns.Count; i++)
            {
                Column column = table.Columns[i];
                if (column.IsIdentity == false && column.IsRowGuidCol == false)
                {
                    //objbank.CompCode = txt_comcode.Text.Trim();
                    streamWriter.WriteLine("\t\t\t\t   obj" + variableName + "." + column.Name.Trim() + " = txt_" + column.Name.Trim() + ".Text.Trim();");
                }
            }




            // Append the method footer
            streamWriter.WriteLine("\t\t");
            streamWriter.WriteLine();
        }

    }
}

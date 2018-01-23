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

			//Utility.WriteResourceToFile("DataTierGenerator.Resources.SharpCore.SharpCore.Data.dll", Path.Combine(sharpCoreDirectory, "SharpCore.Data.dll"));
			//Utility.WriteResourceToFile("DataTierGenerator.Resources.SharpCore.SharpCore.Data.pdb", Path.Combine(sharpCoreDirectory, "SharpCore.Data.pdb"));
			//Utility.WriteResourceToFile("DataTierGenerator.Resources.SharpCore.SharpCore.Extensions.dll", Path.Combine(sharpCoreDirectory, "SharpCore.Extensions.dll"));
			//Utility.WriteResourceToFile("DataTierGenerator.Resources.SharpCore.SharpCore.Extensions.pdb", Path.Combine(sharpCoreDirectory, "SharpCore.Extensions.pdb"));
			//Utility.WriteResourceToFile("DataTierGenerator.Resources.SharpCore.SharpCore.Utilities.dll", Path.Combine(sharpCoreDirectory, "SharpCore.Utilities.dll"));
			//Utility.WriteResourceToFile("DataTierGenerator.Resources.SharpCore.SharpCore.Utilities.pdb", Path.Combine(sharpCoreDirectory, "SharpCore.Utilities.pdb"));
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



        internal static void WebApimethods(string databaseName, Table table, string targetNamespace, string storedProcedurePrefix, string daoSuffix, string dtoSuffix, string path)
        {
            //WebAPI

            string className = Utility.FormatClassName(table.Name) + daoSuffix;
            path = Path.Combine(path, "WebAPI");

            using (StreamWriter streamWriter = new StreamWriter(Path.Combine(path, className + ".cs")))
            {

                CreateWebApiMethods(table, storedProcedurePrefix, dtoSuffix, streamWriter);

            }
        }

        private static void CreateWebApiMethods(Table table, string storedProcedurePrefix, string dtoSuffix, StreamWriter streamWriter)
        {
            string className = Utility.FormatClassName(table.Name) + dtoSuffix;
            string variableName = Utility.FormatVariableName(table.Name);

            streamWriter.WriteLine("\\ className: " + className + " variableName - " + variableName);

            streamWriter.WriteLine("");
            streamWriter.WriteLine("\t");
            streamWriter.WriteLine("\t\t");
            streamWriter.WriteLine("\t\t\t");


            streamWriter.WriteLine("using FSMS.ModelCS;");
            streamWriter.WriteLine("using FSMS.Repository.Factory;");
            streamWriter.WriteLine("using FSMS.Repository.RepositoryDapper;");
            streamWriter.WriteLine("using System;");
            streamWriter.WriteLine("using System.Collections.Generic;");
            streamWriter.WriteLine("using System.Net;");
            streamWriter.WriteLine("using System.Net.Http;");
            streamWriter.WriteLine("using System.Threading.Tasks;");
            streamWriter.WriteLine("using System.Web.Http;");
            streamWriter.WriteLine("using System.Web.Http.Cors;");


            streamWriter.WriteLine("namespace FSMS.UIAspNET.Controllers");
            streamWriter.WriteLine("\t{");
            streamWriter.WriteLine("\t[EnableCors(origins: \" * \", headers: \"*\", methods: \"*\", exposedHeaders: \"Message,Error\")]");
            streamWriter.WriteLine("\t[RoutePrefix(\""+ className + "\")]");
            streamWriter.WriteLine("\tpublic class " + className + "Controller : ApiController");
            streamWriter.WriteLine("\t{");

            streamWriter.WriteLine("\t\tGenericRepository <" + className + "> Repo;");
            streamWriter.WriteLine("\t\tpublic " + className + "Controller()");
            streamWriter.WriteLine("\t\t{");

            streamWriter.WriteLine("\t\t\t Repo = new GenericRepository<" + className + ">(ConnectionFactory.GetOpenConnection(), \"FuelTypes\");");
            streamWriter.WriteLine("\t\t}");

            streamWriter.WriteLine("\t\t/*//////////////////////////////////////////////// Finalize methods /////////////////////////////////////////////////*/");
            streamWriter.WriteLine("\t\t[Route(\"GetAll\")]");
            streamWriter.WriteLine("\t\t[HttpGet]");
            streamWriter.WriteLine("\t\tpublic async Task<HttpResponseMessage> GetAll()");
            streamWriter.WriteLine("\t\t{");
            streamWriter.WriteLine("\t\t\ttry");
            streamWriter.WriteLine("\t\t\t{");
            streamWriter.WriteLine("\t\t\t\t var response = Request.CreateResponse<IEnumerable<" + className + ">>(HttpStatusCode.OK, await Repo.AllAsync());");
            streamWriter.WriteLine("\t\t\t\t response.Headers.Add(\"Message\", \"Passing test values\");");
            streamWriter.WriteLine("\t\t\t\tresponse.Headers.Add(\"Error\", \"null\");");
            streamWriter.WriteLine("\t\t\t\treturn response;");
            streamWriter.WriteLine("\t\t\t}");
            streamWriter.WriteLine("\t\t\t\t catch (Exception ex)");
            streamWriter.WriteLine("\t\t\t\t {");
            streamWriter.WriteLine("\t\t\t\t return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new Exception(\"Error from Get all\", ex));");
            streamWriter.WriteLine("\t\t\t\t}");
            streamWriter.WriteLine("\t\t\t}");


            streamWriter.WriteLine("\t\t        [Route(\"Get\")]");
            streamWriter.WriteLine("\t\t[HttpGet]");
            streamWriter.WriteLine("\t\tpublic async Task<HttpResponseMessage> Get(int id)");
            streamWriter.WriteLine("\t\t{");
            streamWriter.WriteLine("\t\t\ttry");
            streamWriter.WriteLine("\t\t{");
            streamWriter.WriteLine("\t\t\t\tvar stu = await Repo.FindAsync(id);");
            streamWriter.WriteLine("\t\t\t\tif (stu == null)");
            streamWriter.WriteLine("\t\t\t\t{");
            streamWriter.WriteLine("\t\t\t\t    return Request.CreateErrorResponse(HttpStatusCode.NotFound, \"Fuel Type Not found\");");
            streamWriter.WriteLine("\t\t\t\t}");
            streamWriter.WriteLine("\t\t\t\telse");
            streamWriter.WriteLine("\t\t\t\t{");
            streamWriter.WriteLine("\t\t\t\tvar response = Request.CreateResponse<" + className + ">(HttpStatusCode.OK, stu);");
            streamWriter.WriteLine("\t\t\t\tresponse.Headers.Add(\"Message\", \"Passing test values\");");
            streamWriter.WriteLine("\t\t\t\treturn response;");
            streamWriter.WriteLine("\t\t\t\t}");
            streamWriter.WriteLine("\t\t\t}");
            streamWriter.WriteLine("\t\t\tcatch (Exception ex)");
            streamWriter.WriteLine("\t\t\t{");
            streamWriter.WriteLine("\t\t\t\t    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new Exception(\"Error from Get all\", ex));");
            streamWriter.WriteLine("\t\t}");
            streamWriter.WriteLine("\t\t}");


            streamWriter.WriteLine("\t\t [HttpPost, HttpGet]");
            streamWriter.WriteLine("\t\t[Route(\"SaveAsync\")]");
            streamWriter.WriteLine("\t\tpublic async Task<HttpResponseMessage> SaveAsync([FromBody]" + className + " Entity)");
            streamWriter.WriteLine("\t\t{");
            streamWriter.WriteLine("\t\t\ttry");
            streamWriter.WriteLine("\t\t\t{");
            streamWriter.WriteLine("\t\t\t\tint newid = await Repo.AddAsync(Entity);");
            streamWriter.WriteLine("\t\t\t\tvar result = Request.CreateResponse<int>(HttpStatusCode.OK, newid);");
            streamWriter.WriteLine("\t\t\t\tresult.Headers.Add(\"Message\", \"Record Saved Successfully\");");
            streamWriter.WriteLine("\t\t\t\treturn result;");
            streamWriter.WriteLine("\t\t\t}");
            streamWriter.WriteLine("\t\t\tcatch (Exception ex)");
            streamWriter.WriteLine("\t\t\t            {");
            streamWriter.WriteLine("\t\t\t\treturn Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new Exception(\"Custome message here at Save\", ex));");
            streamWriter.WriteLine("\t\t\t}");
            streamWriter.WriteLine("\t\t}");

            streamWriter.WriteLine("\t\t[HttpPost, HttpGet]");
            streamWriter.WriteLine("\t\t[Route(\"UpdateAsync\")]");
            streamWriter.WriteLine("\t\tpublic async Task<HttpResponseMessage> UpdateAsync([FromBody]" + className + " Entity)");
            streamWriter.WriteLine("\t\t{");
            streamWriter.WriteLine("\t\t\ttry");
            streamWriter.WriteLine("\t\t\t{");
            streamWriter.WriteLine("\t\t\t\tbool newid = await Repo.UpdateAsync(Entity, Entity.Id);");
            streamWriter.WriteLine("\t\t\t\tvar result = Request.CreateResponse<bool>(HttpStatusCode.OK, newid);");
            streamWriter.WriteLine("\t\t\t\tresult.Headers.Add(\"Message\", \"Record updated Successfully\");");
            streamWriter.WriteLine("\t\t\t\treturn result;");
            streamWriter.WriteLine("\t\t\t}");
            streamWriter.WriteLine("\t\t\tcatch (Exception ex)");
            streamWriter.WriteLine("\t\t\t{");
            streamWriter.WriteLine("\t\t\t\treturn Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new Exception(\"Custome message here at update\", ex));");
            streamWriter.WriteLine("\t\t\t}");
            streamWriter.WriteLine("\t\t\t}");

            streamWriter.WriteLine("\t\t[HttpPost, HttpGet]");
            streamWriter.WriteLine("\t\t[Route(\"DeleteAsync\")]");
            streamWriter.WriteLine("\t\tpublic async Task<HttpResponseMessage> DeleteAsync(int id)");
            streamWriter.WriteLine("\t\t{");
            streamWriter.WriteLine("\t\t\ttry");
            streamWriter.WriteLine("\t\t\t{");
            streamWriter.WriteLine("\t\t\t\tbool newid = await Repo.RemoveAsync(id);");
            streamWriter.WriteLine("\t\t\t\tif (newid)");
            streamWriter.WriteLine("\t\t\t\t{");
            streamWriter.WriteLine("\t\t\t\tvar result = Request.CreateResponse<bool>(HttpStatusCode.OK, newid);");
            streamWriter.WriteLine("\t\t\t\tresult.Headers.Add(\"Message", "Record updated Successfully\");");
            streamWriter.WriteLine("\t\t\t\treturn result;");
            streamWriter.WriteLine("\t\t\t\t}");
            streamWriter.WriteLine("\t\t\t\telse");
            streamWriter.WriteLine("\t\t\t\t{");
            streamWriter.WriteLine("\t\t\t\t     return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new Exception(\"Record Not found for id:-\" + id.ToString()));") ;
            streamWriter.WriteLine("\t\t\t\t}");
            streamWriter.WriteLine("\t\t\t\t}");
            streamWriter.WriteLine("\t\t\tcatch (Exception ex)");
            streamWriter.WriteLine("\t\t\t{");
            streamWriter.WriteLine("\t\t\treturn Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new Exception(\"Custome message here at update\", ex));");
            streamWriter.WriteLine("\t\t\t}");
            streamWriter.WriteLine("\t\t\t}");

            streamWriter.WriteLine("\t\t [HttpPost, HttpGet]");
            streamWriter.WriteLine("\t\t[Route(\"SaveMultiAsync\")]");
            streamWriter.WriteLine("\t\t public async Task<IHttpActionResult> SaveMultiAsync([FromBody]List<" + className + "> Entity)");
            streamWriter.WriteLine("\t\t{");
            streamWriter.WriteLine("\t\t\t try");
            streamWriter.WriteLine("\t\t\t   {");
            streamWriter.WriteLine("\t\t\t      int newid = await Repo.AddAsync(Entity);");
            streamWriter.WriteLine("\t\t\t      return Ok(newid);");
            streamWriter.WriteLine("\t\t\t   }");
            streamWriter.WriteLine("\t\t\t  catch (Exception ex)");
            streamWriter.WriteLine("\t\t\t {");
            streamWriter.WriteLine("\t\t\t     return InternalServerError(new Exception(\"Custome message here at Save all\", ex));");
            streamWriter.WriteLine("\t\t\t   }");
            streamWriter.WriteLine("\t\t\t}");


            // Append the method footer
            streamWriter.WriteLine("\t\t");
            streamWriter.WriteLine("}");
            streamWriter.WriteLine("}");
        }



        internal static void FrontEndAngular(string databaseName, Table table, string targetNamespace, string storedProcedurePrefix, string daoSuffix, string dtoSuffix, string path)
        {

            string className = Utility.FormatClassName(table.Name) + daoSuffix;
            path = Path.Combine(path, "FrontEndAngular");

            using (StreamWriter streamWriter = new StreamWriter(Path.Combine(path, className + ".ts")))
            {

                CreateFrontEndAngularTS(table, storedProcedurePrefix, dtoSuffix, streamWriter);

            }
        }

        private static void CreateFrontEndAngularTS(Table table, string storedProcedurePrefix, string dtoSuffix, StreamWriter streamWriter)
        {
            string className = Utility.FormatClassName(table.Name) + dtoSuffix;
            string variableName = Utility.FormatVariableName(table.Name);

            streamWriter.WriteLine("// className: " + className + " variableName - " + variableName);

            streamWriter.WriteLine("");
            streamWriter.WriteLine("\t");
            streamWriter.WriteLine("\t\t");
            streamWriter.WriteLine("\t\t\t");

            streamWriter.WriteLine("import { Component, OnInit, ViewChild, AfterViewInit, ViewContainerRef } from '@angular/core';");
            streamWriter.WriteLine("import { "+ className + " } from '../../model/fuelType.model';");
            streamWriter.WriteLine("import { HttpClient } from '@angular/common/http';");
            streamWriter.WriteLine("import { Subject } from 'rxjs/Subject';");
            streamWriter.WriteLine("import { DataTableDirective } from 'angular-datatables';");
            streamWriter.WriteLine("import 'rxjs/add/operator/map';");
            streamWriter.WriteLine("import { FormGroup, FormControl, Validators, FormBuilder } from '@angular/forms';");
            streamWriter.WriteLine("import { GlobalConfig } from '../../service/globalconfig.service';");
            streamWriter.WriteLine("import { HttpHeaders } from '@angular/common/http';");


            streamWriter.WriteLine("@Component({");
            streamWriter.WriteLine("\tselector: 'app-fueltypes',");
            streamWriter.WriteLine("\ttemplateUrl: './"+ variableName + ".component.html',");
            streamWriter.WriteLine("\tstyleUrls: ['./" + variableName + ".component.css']");
            streamWriter.WriteLine("})");

            streamWriter.WriteLine("export class "+ className + "Component implements OnInit, AfterViewInit {");
            streamWriter.WriteLine("\t//Used for Data table object");
            streamWriter.WriteLine("\t@ViewChild(DataTableDirective)");
            streamWriter.WriteLine("\tdtElement: DataTableDirective;");
            streamWriter.WriteLine("\tdtOptions: DataTables.Settings = {};");
            streamWriter.WriteLine("\tdtTrigger: Subject<any> = new Subject();");
            streamWriter.WriteLine("\tdtInstance: DataTables.Api;");
            streamWriter.WriteLine("\t//Confirm and popover messages for insert update delete");
            streamWriter.WriteLine("\tpopoverTitle: string = this.gloconfig.GetmessageCaption;");
            streamWriter.WriteLine("\tpopoverMessageSave: string = this.gloconfig.GetconfirmInsert;");
            streamWriter.WriteLine("\tpopoverMessageUpdate: string = this.gloconfig.GetconfirmModify;");
            streamWriter.WriteLine("\tpopoverMessageDelete: string = this.gloconfig.GetconfirmDelete;");
            streamWriter.WriteLine("\tconfirmText: string = 'Yes <i class=\"glyphicon glyphicon-ok\"></i>';");
            streamWriter.WriteLine("\tcancelText: string = 'No <i class=\"glyphicon glyphicon-remove\"></i>';");

            streamWriter.WriteLine("private customHeaders: HttpHeaders = this.setCredentialsHeader();");
            streamWriter.WriteLine("\tmyform: FormGroup;");
            streamWriter.WriteLine("\tselectedRow: any;");
            streamWriter.WriteLine("\tselectedItem:FuelType;");
            streamWriter.WriteLine("\tholdvar: FuelType[] = [];");
            streamWriter.WriteLine("\tfilterholder: FuelType[];");
            streamWriter.WriteLine("\tobj: FuelType = new FuelType();");
            streamWriter.WriteLine("\t//Variables for error and sucess messages");
            streamWriter.WriteLine("\tissuccess = false;");
            streamWriter.WriteLine("\tiserror = false;");
            streamWriter.WriteLine("\tsuccessmsg = \"Inisizlising success message\";");
            streamWriter.WriteLine("\terrormsg = \"Inisizlising success message\";");
            streamWriter.WriteLine("\t//************************************************************************** constructor ***************************************");
            streamWriter.WriteLine("\tconstructor(private _http: HttpClient, private gloconfig: GlobalConfig,");
            streamWriter.WriteLine("\tprivate formBuilder: FormBuilder) {}");

            streamWriter.WriteLine("\t//************************************************************************** Messaging MEthods ***************************************");
            streamWriter.WriteLine(" showSuccess(message: string) {");
            streamWriter.WriteLine("\tthis.issuccess = true;");
            streamWriter.WriteLine("\tthis.iserror = false;");
            streamWriter.WriteLine("\tthis.successmsg = message;");
            streamWriter.WriteLine("\tsetTimeout(() => {");
            streamWriter.WriteLine("\t\tthis.issuccess = false;");
            streamWriter.WriteLine("\t\tthis.iserror = false;");
            streamWriter.WriteLine("\t}, 5000);");
            streamWriter.WriteLine("\tthis.selectedItem = new FuelType();");
            streamWriter.WriteLine("}");
            streamWriter.WriteLine("showError(message: string) {");
            streamWriter.WriteLine("\t\tthis.errormsg = message");
            streamWriter.WriteLine("\t\tthis.issuccess = false;");
            streamWriter.WriteLine("\t\tthis.iserror = true;");
            streamWriter.WriteLine("\t\tsetTimeout(() => {");
            streamWriter.WriteLine("\t\t\tthis.issuccess = false;");
            streamWriter.WriteLine("\t\t\tthis.iserror = false;");
            streamWriter.WriteLine("\t\t                }, 5000);");
            streamWriter.WriteLine("\t}");

            streamWriter.WriteLine("\t//************************************************************************** Validations ***************************************");
            streamWriter.WriteLine("//Validation"); 
            streamWriter.WriteLine("\tisFieldValid(field: string) {");
            streamWriter.WriteLine("\t\treturn !this.myform.get(field).valid && this.myform.get(field).touched;");
            streamWriter.WriteLine("\t}");
            streamWriter.WriteLine("\tdisplayFieldCss(field: string) {");
            streamWriter.WriteLine("\t\treturn {");
            streamWriter.WriteLine("\t'has-error': this.isFieldValid(field),");
            streamWriter.WriteLine("\t'has-feedback': this.isFieldValid(field)");
            streamWriter.WriteLine("\t};");
            streamWriter.WriteLine("\t}");

            streamWriter.WriteLine("\t//************************************************************************** OnInit ***************************************");
            streamWriter.WriteLine("\tngOnInit() {");
            streamWriter.WriteLine("\tthis.dtOptions = {");
            streamWriter.WriteLine("\t\tpagingType: 'full_numbers',");
            streamWriter.WriteLine("\t\tpageLength: 10,");
            streamWriter.WriteLine("\t};");
            streamWriter.WriteLine("\tthis.myform = this.formBuilder.group({");
            streamWriter.WriteLine("\t\tId: [null],");
            streamWriter.WriteLine("\t\tFuelFullName: [null, Validators.required],");
            streamWriter.WriteLine("\t\tFuelShortName: [null, [Validators.required, Validators.maxLength(10)]],");
            streamWriter.WriteLine("\t\tUnitPrice: [null, Validators.required],");
            streamWriter.WriteLine("\t});");
            streamWriter.WriteLine("\tthis.Filter();");
            streamWriter.WriteLine("\tthis.switchData();");
            streamWriter.WriteLine("\tthis.showSuccess(\"Program Inisialized\");");
            streamWriter.WriteLine("\t}");

            streamWriter.WriteLine("\t//************************************************************************** ngAfterViewInit ***************************************");
            streamWriter.WriteLine("\tngAfterViewInit(): void {");
            streamWriter.WriteLine("\t\tconsole.log(\"ngAfterViewInit\", this.holdvar);");
            streamWriter.WriteLine("\t\tthis.dtTrigger.next();");
            streamWriter.WriteLine("\t}");
            streamWriter.WriteLine("\t");
            streamWriter.WriteLine("\tsetCredentialsHeader() {");
            streamWriter.WriteLine("\t\tlet headers = new HttpHeaders();");
            streamWriter.WriteLine("\t\tlet credentials = window.localStorage.getItem('credentials2');");
            streamWriter.WriteLine("\t\tlet token = \"RcF2kW7g6KKscCTR3 -YoMJjzhAPxCXufe3fy2NXiIlm8NGtUqbrvzQtCcrIByxNqmav_vFacZmhAX22A8MRnl6JCy6ATUDeAz-dE_H6pHgQzGbYK0pbKv06H3a-QGiYsM-a5ASlLEbe1lRD4cGVmkpVBoIdIj6Qw9H9QvZPaZP8o2bnVCxD8ag8ceinYKPYxHKKdO8JsPxjuMk_T1Vlm39vPGYDJC5_45xgF4jcsqxoNLy95bHUhSzvZgsf2jMqG-dwutfcZOAxCtfZ-1FYRvpjre3TWvqySdx59GW5WKpGqbRjZJuvoBLMDQu20fj4pxwzRXYTOvj12GfJ_Vgj9Rz_bCKHkChaBxRm2--UF6CG3okVTSaqPcyqJ0q-PqIDUqa3E23CWOm9v20XzhiLjiUk6gz8kFgomu1zHQibQXa9mLw_N9ATdYp1xXfqCkd7SukmeCmTT-0r_sQJYHFXxhUUUuqeCzXmEf3RpkX5xytjFExOrLbgpN6vIu772FMWO\"");
            streamWriter.WriteLine("\t\theaders.append('Authorization', 'Bearer ' + token);");
            streamWriter.WriteLine("\t\treturn headers;");
            streamWriter.WriteLine("\t}");

            streamWriter.WriteLine("\t//************************************************************************** Filter ***************************************");
            streamWriter.WriteLine("\tFilter() {");
            streamWriter.WriteLine("\t\tlet hed: HttpHeaders = new HttpHeaders();");
            streamWriter.WriteLine("\t\tthis._http.get<"+ className + "[]>(this.gloconfig.GetConnection(\""+className+"\", \"GetAll\"), { headers: this.customHeaders })");
            streamWriter.WriteLine("\t\t.subscribe(");
            streamWriter.WriteLine("\t\tdata => {");
            streamWriter.WriteLine("\t\tthis.filterholder = data;");
            streamWriter.WriteLine("\t\t},");
            streamWriter.WriteLine("\t\terr => {");
            streamWriter.WriteLine("\t\tconsole.log(err)");
            streamWriter.WriteLine("\t\t},");
            streamWriter.WriteLine("\t\t() => {");
            streamWriter.WriteLine("\t\tthis.holdvar = this.filterholder;");
            streamWriter.WriteLine("\t\tthis.switchData();");
            streamWriter.WriteLine("\t\tconsole.log(\"Finish\", this.holdvar)");
            streamWriter.WriteLine("\t\t});");
            streamWriter.WriteLine("\t}");
            streamWriter.WriteLine("\t//************************************************************************** tswitchData ***************************************");
            streamWriter.WriteLine("\tswitchData(): void {");
            streamWriter.WriteLine("\t//in first call on OnInit this.dtElement.dtInstance is not construct and check it for undefinned");
            streamWriter.WriteLine("\t\tif (this.dtElement.dtInstance !== undefined)");
            streamWriter.WriteLine("\t\t{");
            streamWriter.WriteLine("\t\t\tthis.dtElement.dtInstance.then((dtInstance: DataTables.Api) => {");
            streamWriter.WriteLine("\t\t\t// Destroy the table first");
            streamWriter.WriteLine("\t\t\tdtInstance.destroy();");
            streamWriter.WriteLine("\t\t\t// Switch");
            streamWriter.WriteLine("\t\t\tthis.holdvar = this.filterholder; //this.data[id];");
            streamWriter.WriteLine("\t\t\t// Call the dtTrigger to rerender again");
            streamWriter.WriteLine("\t\t\tthis.dtTrigger.next();");
            streamWriter.WriteLine("\t\t});");
            streamWriter.WriteLine("\t\t}");
            streamWriter.WriteLine("\t}");
            streamWriter.WriteLine("\t//************************************************************************** setClickedRow ***************************************");

            streamWriter.WriteLine("\tsetClickedRow(item: any, i: any) {");
                streamWriter.WriteLine("\t\tthis.selectedRow = i;");
            streamWriter.WriteLine("\t\tthis.selectedItem = item;");
            streamWriter.WriteLine("\t}");

            streamWriter.WriteLine("\tonSubmit(myform, event, btn) {");
            streamWriter.WriteLine("\t\tthis.obj.Id = myform.value.Id");
            // Append the parameter declarations
            for (int i = 0; i < table.Columns.Count; i++)
            {
                Column column = table.Columns[i];
                if (column.IsIdentity == false && column.IsRowGuidCol == false)
                {

                    switch (column.Name.Trim()) {
                        case "CreatedDate":
                            streamWriter.WriteLine("\t\tthis.obj.CreatedDate = new Date();");
                            break;
                        case "ModifiedDate":
                            streamWriter.WriteLine("\t\tthis.obj.ModifiedDate = new Date();");
                            break;
                        case "CreatedUser":
                            streamWriter.WriteLine("\t\tthis.obj.CreatedUser = this.gloconfig.GetlogedInUserID;");
                            break;
                        case "ModifiedUser":
                            streamWriter.WriteLine("\t\tthis.obj.ModifiedUser = this.gloconfig.GetlogedInUserID");
                            break;
                        case "DataTransfer":
                            streamWriter.WriteLine("\t\tthis.obj.DataTransfer = 1;");
                            break;
                        default:
                            streamWriter.WriteLine("\t\tthis.obj." + column.Name.Trim() + " = myform.value." + column.Name.Trim() + "");
                            break;
                    }
                }
            }
            streamWriter.WriteLine("\t\tswitch (btn)");
            streamWriter.WriteLine("\t\t{");
            streamWriter.WriteLine("\t\tcase 'Insert':");
            streamWriter.WriteLine("\t\tconsole.log(\"On Submit - case insert : \", btn)");
            streamWriter.WriteLine("\t\tthis.obj.Id = -1;");
            streamWriter.WriteLine("\t\tbreak;");
            streamWriter.WriteLine("\t\tdefault: break;");
            streamWriter.WriteLine("\t\t}");
            streamWriter.WriteLine("\t\t}");

            streamWriter.WriteLine("\t//************************************************************************** SAVE CONFIRM ***************************************");

            streamWriter.WriteLine("\tSaveConfirm() {");
            streamWriter.WriteLine("\t\tif (this.myform.valid)");
            streamWriter.WriteLine("\t\t{");
            streamWriter.WriteLine("\t\t\tthis.Save(this.obj);");
            streamWriter.WriteLine("\t\t\tconsole.log(\"Saving\", this.obj);");
            streamWriter.WriteLine("\t\t}");
            streamWriter.WriteLine("\t\telse");
            streamWriter.WriteLine("\t\t{");
            streamWriter.WriteLine("\t\t\tObject.keys(this.myform.controls).forEach(field => { ");
            streamWriter.WriteLine("\t\t\tconsole.log('form errors', field);");
            streamWriter.WriteLine("\t\t\tconst control = this.myform.get(field);");
            streamWriter.WriteLine("\t\t\tcontrol.markAsTouched({ onlySelf: true });");
            streamWriter.WriteLine("\t\t});");
            streamWriter.WriteLine("\t}");
            streamWriter.WriteLine("\t}");

            streamWriter.WriteLine("\tSaveCancel() {");
            streamWriter.WriteLine("\t\tconsole.log(\"User try to Insert. but cancelled\");");
            streamWriter.WriteLine("\t            }");


            streamWriter.WriteLine("\t//************************************************************************** SAVE ***************************************");
            streamWriter.WriteLine("\tSave(item: "+ className + ") {");
            streamWriter.WriteLine("\tconsole.log(\"Save Confirmed!\", this.myform.valid);");
            streamWriter.WriteLine("\t\t   this._http.post(this.gloconfig.GetConnection(\""+ className + "\", \"SaveAsync\"), item)");
            streamWriter.WriteLine("\t\t\t  .subscribe(");
            streamWriter.WriteLine("\t\t\tdata => {");
            streamWriter.WriteLine("\t\t\tconsole.log(data)");
            streamWriter.WriteLine("\t\t\tthis.showSuccess(\"Record Inserted SuccessFully!\");");
            streamWriter.WriteLine("\t\t},");
            streamWriter.WriteLine("\t\t\terr => {");
            streamWriter.WriteLine("\t\t\tthis.showError(\"Some Error occured while transaction! Record not Inserted!\");");
            streamWriter.WriteLine("\t\t},");
            streamWriter.WriteLine("\t\t() => {");
            streamWriter.WriteLine("\t\t\tconsole.log(\"Finish\")");
            streamWriter.WriteLine("\t\t\tthis.Filter();");
            streamWriter.WriteLine("\t\t\tthis.switchData();");
            streamWriter.WriteLine("\t\t})");
            streamWriter.WriteLine("\t}");

            streamWriter.WriteLine("\t//************************************************************************** UPDATE CONFIRM ***************************************");
            streamWriter.WriteLine("\tUpdateConfirm() {");
            streamWriter.WriteLine("\t\tif (this.myform.valid)");
            streamWriter.WriteLine("\t\t{");
            streamWriter.WriteLine("\t\tthis.Update(this.obj);");
            streamWriter.WriteLine("\t\tconsole.log(\"Saving   \", this.obj);");
            streamWriter.WriteLine("\t\tthis.switchData();");
            streamWriter.WriteLine("\t\t}");
            streamWriter.WriteLine("\t\telse");
            streamWriter.WriteLine("\t\t{");
            streamWriter.WriteLine("\t\tObject.keys(this.myform.controls).forEach(field => {");
            streamWriter.WriteLine("\t\tconsole.log('form errors', field);");
            streamWriter.WriteLine("\t\tconst control = this.myform.get(field);");
            streamWriter.WriteLine("\t\tcontrol.markAsTouched({ onlySelf: true });");
            streamWriter.WriteLine("\t\t});");
            streamWriter.WriteLine("\t}");
            streamWriter.WriteLine("\t}");

            streamWriter.WriteLine("\tUpdateCancel()");
            streamWriter.WriteLine("\t\t{");
            streamWriter.WriteLine("\t\tconsole.log(\"User try to update. but cancelled\");");
            streamWriter.WriteLine("\t\t}");
            streamWriter.WriteLine("\t//************************************************************************** UPDATE ***************************************");
            streamWriter.WriteLine("\t\tUpdate(item: "+ className + ")");
            streamWriter.WriteLine("\t\t{");
            streamWriter.WriteLine("\t\tthis._http.post(this.gloconfig.GetConnection("+className+", \"UpdateAsync\"), item)");
            streamWriter.WriteLine("\t\t.subscribe(");
            streamWriter.WriteLine("\t\tdata => {");
            streamWriter.WriteLine("\t\tconsole.log(data)");
            streamWriter.WriteLine("\t\tif (data === true)");
            streamWriter.WriteLine("\t\t{");
            streamWriter.WriteLine("\t\tthis.showSuccess(\"Record Updated SuccessFully!\");");
            streamWriter.WriteLine("\t\t}");
            streamWriter.WriteLine("\t\t},");
            streamWriter.WriteLine("\t\terr => {");
            streamWriter.WriteLine("\t\tthis.showError(\"Some Error occured while transaction! Record not Updated!\");");
            streamWriter.WriteLine("\t\tconsole.log(err)");
            streamWriter.WriteLine("\t\t},");
            streamWriter.WriteLine("\t\t() => {");
            streamWriter.WriteLine("\t\tconsole.log(\"Finish\")");
            streamWriter.WriteLine("\t\tthis.Filter();");
            streamWriter.WriteLine("\t\tthis.switchData();");
            streamWriter.WriteLine("\t\t});");
            streamWriter.WriteLine("\t\t}");

            streamWriter.WriteLine("\t//************************************************************************** DELETE CONFIRM ***************************************");
            streamWriter.WriteLine("\t\tdeleteConfirm(item: FuelType) {");
            streamWriter.WriteLine("\t\tconsole.log(\"Deleting:-this.selectedItem  \" + item.Id)");
            streamWriter.WriteLine("\t\tthis.Delete(item.Id);");
            streamWriter.WriteLine("\t\t}");
            streamWriter.WriteLine("\t\tdeleteCancel() {");
            streamWriter.WriteLine("\t\tconsole.log(\"User try to Delete. but cancelled\");");
            streamWriter.WriteLine("\t\t}");
            streamWriter.WriteLine("\t\t");
            streamWriter.WriteLine("\t\tDelete(id: number) {");
            streamWriter.WriteLine("\t\tthis._http.post(this.gloconfig.GetConnection(\""+ className + "\", \"DeleteAsync\") + `?id =${ id}`, id)");
            streamWriter.WriteLine("\t\t.subscribe(");
            streamWriter.WriteLine("\t\tdata => {");
            streamWriter.WriteLine("\t\tconsole.log(data)");
            streamWriter.WriteLine("\t\tif (data === true)");
            streamWriter.WriteLine("\t\t{");
            streamWriter.WriteLine("\t\tthis.showSuccess(\"Record Deleted SuccessFully!\");");
            streamWriter.WriteLine("\t\t}");
            streamWriter.WriteLine("\t\t},");
            streamWriter.WriteLine("\t\terr => {");
            streamWriter.WriteLine("\t\tthis.showError(\"Some Error occured while transaction! Record not deleted!\");");
            streamWriter.WriteLine("\t\tconsole.log(err)");
            streamWriter.WriteLine("\t\t},");
            streamWriter.WriteLine("\t\t() => {");
            streamWriter.WriteLine("\t\tconsole.log(\"Finish\")");
            streamWriter.WriteLine("\t\tthis.Filter();");
            streamWriter.WriteLine("\t\tthis.switchData();");
            streamWriter.WriteLine("\t\t});");
            streamWriter.WriteLine("\t\t}");



            //            export class FuelType
            //        {
            //            Id:number;
            //	FuelFullName:string;
            //	FuelShortName:string;
            //	UnitPrice:number;
            //	GroupOfCompanyID:number;
            //	CreatedUser:number;
            //	CreatedDate:Date
            //    ModifiedUser:number;
            //	ModifiedDate:Date
            //    DataTransfer:number;

            //}

            streamWriter.WriteLine("\texport class "+ className.Trim());
            streamWriter.WriteLine("\t{");

            ///////////////////////////////////CLASS FILES //////////////////////////////////////////////
            for (int i = 0; i < table.Columns.Count; i++)
            {
                Column column = table.Columns[i];
                string tstype =  Utility.GetTSType(column);
                streamWriter.WriteLine("\t\t\t "+ column.Name + ":" + tstype + ";");
            }
            streamWriter.WriteLine("\t}");




    }//end of method




      

        internal static void FrontEndAngularHTML(string databaseName, Table table, string targetNamespace, string storedProcedurePrefix, string daoSuffix, string dtoSuffix, string path)
        {
            string className = Utility.FormatClassName(table.Name) + daoSuffix;
            path = Path.Combine(path, "FrontEndAngularHTML");

            using (StreamWriter streamWriter = new StreamWriter(Path.Combine(path, className + ".html")))
            {

                CreateFrontEndAngularHTML(table, storedProcedurePrefix, dtoSuffix, streamWriter);

            }
        }

        private static void CreateFrontEndAngularHTML(Table table, string storedProcedurePrefix, string dtoSuffix, StreamWriter streamWriter)
        {
            string className = Utility.FormatClassName(table.Name) + dtoSuffix;
            string variableName = Utility.FormatVariableName(table.Name);

            streamWriter.WriteLine("\t\t<div class=\"row clearfix\">");
            streamWriter.WriteLine("\t\t<div class=\"card\">");
            streamWriter.WriteLine("\t\t<div class=\"header bg-teal\">");
            streamWriter.WriteLine("\t\t<h2>ALL FUEL TYPES(????? ???? )</h2>");
            streamWriter.WriteLine("\t\t<ul class=\"header-dropdown m-r--5\">");
            streamWriter.WriteLine("\t\t<li class=\"dropdown\">");
            streamWriter.WriteLine("\t\t<a href=\"javascript:void(0);\" class=\"dropdown-toggle\" data-toggle=\"dropdown\" role=\"button\" aria-haspopup=\"true\" aria-expanded=\"false\">");
            streamWriter.WriteLine("\t\t<i class=\"material-icons\">more_vert</i>");
            streamWriter.WriteLine("\t\t</a>");
            streamWriter.WriteLine("\t\t<ul class=\"dropdown-menu pull-right\">");
            streamWriter.WriteLine("\t\t<li>");
            streamWriter.WriteLine("\t\t<a href=\"javascript:void(0);\" class=\" waves-effect waves-block\">Add Fuel Type</a>");
            streamWriter.WriteLine("\t\t</li>");
            streamWriter.WriteLine("\t\t<li>");
            streamWriter.WriteLine("\t\t<a href=\"javascript:void(0);\" class=\" waves-effect waves-block\">Delete Slected</a>");
            streamWriter.WriteLine("\t\t</li>");
            streamWriter.WriteLine("\t\t<li>");
            streamWriter.WriteLine("\t\t<a href=\"javascript:void(0);\" class=\" waves-effect waves-block\">View Full Details</a>");
            streamWriter.WriteLine("\t\t</li>");
            streamWriter.WriteLine("\t\t</ul>");
            streamWriter.WriteLine("\t\t</li>");
            streamWriter.WriteLine("\t\t</ul>");
            streamWriter.WriteLine("\t\t</div>");
            streamWriter.WriteLine("\t\t");
            streamWriter.WriteLine("\t\t<div class=\"body\">");
            streamWriter.WriteLine("\t\t");
            streamWriter.WriteLine("\t\t");
            streamWriter.WriteLine("\t\t<div class=\"row clearfix\">");
            streamWriter.WriteLine("\t\t<div class=\"col-xs-12 col-sm-12 col-md-8 col-lg-8\">");
            streamWriter.WriteLine("\t\t<div *ngIf=\"issuccess\" class=\"alert alert-success\">");
            streamWriter.WriteLine("\t\t<strong>Well done!</strong> {{successmsg}}");
            streamWriter.WriteLine("\t\t</div>");
            streamWriter.WriteLine("\t\t");
            streamWriter.WriteLine("\t\t<div *ngIf=\"iserror\" class=\"alert alert-danger\">");
            streamWriter.WriteLine("\t\t<strong>Ooops!</strong> {{errormsg}}");
            streamWriter.WriteLine("\t\t</div>");
            streamWriter.WriteLine("\t\t</div>");
            streamWriter.WriteLine("\t\t</div>");
            streamWriter.WriteLine("\t\t");
            streamWriter.WriteLine("\t\t<div class=\"row clearfix\">");
            streamWriter.WriteLine("\t\t");
            streamWriter.WriteLine("\t\t<!-- Task Info -->");
            streamWriter.WriteLine("\t\t<div class=\"col-xs-12 col-sm-12 col-md-8 col-lg-8\">");
            streamWriter.WriteLine("\t\t<div class=\"card\">");
            streamWriter.WriteLine("\t\t<div class=\"body\">");
            streamWriter.WriteLine("\t\t<div class=\"table-responsive\">");
            streamWriter.WriteLine("\t\t<table datatable [dtOptions]=\"dtOptions\" [dtTrigger]=\"dtTrigger\" class=\"table table-hover dashboard-task-infos\">");
            streamWriter.WriteLine("\t\t<thead>");
            streamWriter.WriteLine("\t\t<tr>");
            streamWriter.WriteLine("\t\t<th>#</th>");
            streamWriter.WriteLine("\t\t<th>Fuel Name</th>");
            streamWriter.WriteLine("\t\t<th>Fuel ShortName</th>");
            streamWriter.WriteLine("\t\t<th>Unit Price</th>");
            streamWriter.WriteLine("\t\t<th></th>");
            streamWriter.WriteLine("\t\t</tr>");
            streamWriter.WriteLine("\t\t</thead>");
            streamWriter.WriteLine("\t\t<tbody *ngIf=\"holdvar\">");
            streamWriter.WriteLine("\t\t<tr *ngFor=\"let item of holdvar;  let i = index\" (click)=\"setClickedRow(item,i)\" [class.active]=\"i == selectedRow\">");
            streamWriter.WriteLine("\t\t<td>{{item.Id}}</td>");
            streamWriter.WriteLine("\t\t<td>{{item.FuelFullName}}</td>");
            streamWriter.WriteLine("\t\t<td>{{item.FuelShortName}}</td>");
            streamWriter.WriteLine("\t\t<td>");
            streamWriter.WriteLine("\t\t{{item.UnitPrice}}");
            streamWriter.WriteLine("\t\t</td>");
            streamWriter.WriteLine("\t\t<td>");
            streamWriter.WriteLine("\t\t<a class=\"btn  btn-xs btn-danger waves-effect\" mwlConfirmationPopover [popoverTitle]=\"popoverTitle\" [popoverMessage]=\"popoverMessageDelete\"");
            streamWriter.WriteLine("\t\t[confirmText]=\"confirmText\" [cancelText]=\"cancelText\" [placement]=\"top\" (confirm)=\"deleteConfirm(item)\"");
            streamWriter.WriteLine("\t\t(cancel)=\"deleteCancel()\" confirmButtonType=\"danger\" cancelButtonType=\"default\" [appendToBody]=\"true\">");
            streamWriter.WriteLine("\t\tX </a>");
            streamWriter.WriteLine("\t\t</td>");
            streamWriter.WriteLine("\t\t</tr>");
            streamWriter.WriteLine("\t\t");
            streamWriter.WriteLine("\t\t");
            streamWriter.WriteLine("\t\t</tbody>");
            streamWriter.WriteLine("\t\t</table>");
            streamWriter.WriteLine("\t\t</div>");
            streamWriter.WriteLine("\t\t</div>");
            streamWriter.WriteLine("\t\t</div>");
            streamWriter.WriteLine("\t\t</div>");
            streamWriter.WriteLine("\t\t<!-- #END# Task Info -->");



            streamWriter.WriteLine("\t\t<div class=\"col-xs-12 col-sm-12 col-md-4 col-lg-4\">");
            streamWriter.WriteLine("\t\t<div class=\"card\">");
            streamWriter.WriteLine("\t\t<div class=\"body\">");
            streamWriter.WriteLine("\t\t<form id=\"form_advanced_validation\" [formGroup]=\"myform\" (onSubmit)=\"onSubmit(myform,$event,'Insert')\">");
            streamWriter.WriteLine("\t\t<input type=\"text\" formControlName=\"Id\" [ngModel]=\"selectedItem.Id\" style=\"display:none\">");
            streamWriter.WriteLine("\t\t<label for=\"email_address\">Fuel Name (????? ?????? ??)</label>");
            streamWriter.WriteLine("\t\t<div class=\"form-group\" [ngClass]=\"displayFieldCss('FuelFullName')\">");
            streamWriter.WriteLine("\t\t<div class=\"form-line\">");
            streamWriter.WriteLine("\t\t<input type=\"text\" required id=\"email_address\" formControlName=\"FuelFullName\" [ngModel]=\"selectedItem.FuelFullName\" class=\"form-control\"");
            streamWriter.WriteLine("\t\tplaceholder=\"Fuel Name (????? ?????? ??)\" [ngClass]=\"displayFieldCss('FuelFullName')\">");
            streamWriter.WriteLine("\t\t</div>");
            streamWriter.WriteLine("\t\t<div>");
            streamWriter.WriteLine("\t\t<app-field-error-display [displayError]=\"isFieldValid('FuelFullName')\" errorMsg=\"Fuel Name (????? ?????? ??) required!\">");
            streamWriter.WriteLine("\t\t</app-field-error-display>");
            streamWriter.WriteLine("\t\t</div>");
            streamWriter.WriteLine("\t\t</div>");
            streamWriter.WriteLine("\t\t<label for=\"email_address\">Fuel Short Name (???? ?? )</label>");
            streamWriter.WriteLine("\t\t<div class=\"form-group\" [ngClass]=\"displayFieldCss('FuelShortName')\">");
            streamWriter.WriteLine("\t\t<div class=\"form-line\">");
            streamWriter.WriteLine("\t\t<input type=\"text\" id=\"email_address\" formControlName=\"FuelShortName\" [ngModel]=\"selectedItem.FuelShortName\" class=\"form-control\"");
            streamWriter.WriteLine("\t\tplaceholder=\"Fuel Short Name (???? ?? )\">");
            streamWriter.WriteLine("\t\t</div>");
            streamWriter.WriteLine("\t\t<app-field-error-display [displayError]=\"isFieldValid('FuelShortName')\" errorMsg=\"This Field Is reqired! 10 Charactors Maximum\">");
            streamWriter.WriteLine("\t\t</app-field-error-display>");
            streamWriter.WriteLine("\t\t</div>");
            streamWriter.WriteLine("\t\t<label for=\"email_address\">Unit Price (????? ???)</label>");
            streamWriter.WriteLine("\t\t<div class=\"form-group\">");
            streamWriter.WriteLine("\t\t<div class=\"form-line\">");
            streamWriter.WriteLine("\t\t<input type=\"number\" id=\"email_address\" formControlName=\"UnitPrice\" [ngModel]=\"selectedItem.UnitPrice\" class=\"form-control\"");
            streamWriter.WriteLine("\t\tplaceholder=\"Unit Price (????? ???)\">");
            streamWriter.WriteLine("\t\t</div>");
            streamWriter.WriteLine("\t\t<app-field-error-display [displayError]=\"isFieldValid('UnitPrice')\" errorMsg=\"This Field Is reqired! Numbers Only!\">");
            streamWriter.WriteLine("\t\t</app-field-error-display>");
            streamWriter.WriteLine("\t\t</div>");
            streamWriter.WriteLine("\t\t<button class=\"btn btn-primary\" mwlConfirmationPopover [popoverTitle]=\"popoverTitle\" [popoverMessage]=\"popoverMessageSave\"");
            streamWriter.WriteLine("\t\t[confirmText]=\"confirmText\" [cancelText]=\"cancelText\" [placement]=\"bottom\" (confirm)=\"SaveConfirm()\" (cancel)=\"SaveCancel()\"");
            streamWriter.WriteLine("\t\tconfirmButtonType=\"info\" cancelButtonType=\"default\" (click)=\"onSubmit(myform,$event,'Insert')\" [appendToBody]=\"true\">");
            streamWriter.WriteLine("\t\tInsert {{ placement }}");
            streamWriter.WriteLine("\t\t</button>");
            streamWriter.WriteLine("\t\t<button class=\"btn btn-primary\" mwlConfirmationPopover [popoverTitle]=\"popoverTitle\" [popoverMessage]=\"popoverMessageUpdate\"");
            streamWriter.WriteLine("\t\t[confirmText]=\"confirmText\" [cancelText]=\"cancelText\" [placement]=\"bottom\" (confirm)=\"UpdateConfirm()\" (cancel)=\"UpdateCancel()\"");
            streamWriter.WriteLine("\t\tconfirmButtonType=\"warning\" cancelButtonType=\"default\" (click)=\"onSubmit(myform,$event,'Update')\" [appendToBody]=\"true\">");
            streamWriter.WriteLine("\t\tUpdate {{ placement }}");
            streamWriter.WriteLine("\t\t</button>");
            streamWriter.WriteLine("\t\t</form>");
            streamWriter.WriteLine("\t\t</div>");
            streamWriter.WriteLine("\t\t</div>");
            streamWriter.WriteLine("\t\t</div>");
            streamWriter.WriteLine("\t\t</div>");
            streamWriter.WriteLine("\t\t</div>");
            streamWriter.WriteLine("\t\t</div>");
            streamWriter.WriteLine("\t\t<!-- #END# Browser Usage -->");
            streamWriter.WriteLine("\t\t</div>");



            streamWriter.WriteLine("\t\t<div class=\"col-xs-12 col-sm-12 col-md-4 col-lg-4\">");
            streamWriter.WriteLine("\t\t<div class=\"card\">");
            streamWriter.WriteLine("\t\t<div class=\"body\">");
            streamWriter.WriteLine("\t\t<form id=\"form_advanced_validation\" [formGroup]=\"myform\" (onSubmit)=\"onSubmit(myform,$event,'Insert')\">");
            streamWriter.WriteLine("\t\t<input type=\"text\" formControlName=\"Id\" [ngModel]=\"selectedItem.Id\" style=\"display:none\">");
            streamWriter.WriteLine("\t\t");
            streamWriter.WriteLine("\t\t");
            streamWriter.WriteLine("\t\t");

            for (int i = 0; i < table.Columns.Count; i++)
            {
                Column column = table.Columns[i];
                if (column.IsIdentity == false && column.IsRowGuidCol == false)
                {
                    streamWriter.WriteLine("\t\t<label for=\""+ column.Name.Trim() + "\">"+ column.Name.Trim() + " (????? ?????? ??)</label>");
                    streamWriter.WriteLine("\t\t<div class=\"form-group\" [ngClass]=\"displayFieldCss('"+ column.Name.Trim() + "')\">");
                    streamWriter.WriteLine("\t\t<div class=\"form-line\">");
                    streamWriter.WriteLine("\t\t<input type=\"text\" required id=\"ID_"+ column.Name.Trim() + "\" formControlName=\""+ column.Name.Trim() + "\" [ngModel]=\"selectedItem."+ column.Name.Trim() + "\" class=\"form-control\"");
                    streamWriter.WriteLine("\t\tplaceholder=\""+ column.Name.Trim()  + " (????? ?????? ??)\" [ngClass]=\"displayFieldCss('FuelFullName')\">");
                    streamWriter.WriteLine("\t\t</div>");
                    streamWriter.WriteLine("\t\t<div>");
                    streamWriter.WriteLine("\t\t<app-field-error-display [displayError]=\"isFieldValid('"+ column.Name.Trim() + "')\" errorMsg=\""+ column.Name.Trim() + " (????? ?????? ??) required!\">");
                    streamWriter.WriteLine("\t\t</app-field-error-display>");
                    streamWriter.WriteLine("\t\t</div>");
                    streamWriter.WriteLine("\t\t</div>");
                }
            }

            streamWriter.WriteLine("\t\t");
            streamWriter.WriteLine("\t\t");
            streamWriter.WriteLine("\t\t");
            streamWriter.WriteLine("\t\t</form>");
            streamWriter.WriteLine("\t\t</div>");
            streamWriter.WriteLine("\t\t</div>");
            streamWriter.WriteLine("\t\t</div>");
            streamWriter.WriteLine("\t\t</div>");
            streamWriter.WriteLine("\t\t</div>");
            streamWriter.WriteLine("\t\t</div>");
            streamWriter.WriteLine("\t\t<!-- #END# Browser Usage -->");
            streamWriter.WriteLine("\t\t</div>");








        }
    }//end of class
}//end of namespace

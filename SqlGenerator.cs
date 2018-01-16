using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DataTierGenerator
{
	/// <summary>
	/// Generates SQL Server stored procedures for a database.
	/// </summary>
	internal static class SqlGenerator
	{
        /// <summary>
        /// Creates the "use [database]" statement in a specified file.
        /// </summary>
		/// <param name="databaseName">The name of the database that the login will be created for.</param>
		/// <param name="path">Path where the "use [database]" statement should be created.</param>
		/// <param name="createMultipleFiles">Indicates if the script should be created in its own file.</param>
        public static void CreateUseDatabaseStatement(string databaseName, string path, bool createMultipleFiles)
        {
            if (!createMultipleFiles)
            {
				string fileName = Path.Combine(path, "StoredProcedures.sql");
		        using (StreamWriter streamWriter = new StreamWriter(fileName, true))
		        {
                    streamWriter.WriteLine("use [{0}]", databaseName);
                    streamWriter.WriteLine("go");
                }
            }
        }
        /// <summary>
        /// Writes the "use [database]" statement to the specified stream.
        /// </summary>
		/// <param name="databaseName">The name of the database that the login will be created for.</param>
		/// <param name="streamWriter">StreamWriter to write the script to.</param>
        public static void CreateUseDatabaseStatement(string databaseName, StreamWriter streamWriter)
        {
            streamWriter.WriteLine("use [{0}]", databaseName);
            streamWriter.WriteLine("go");
        }

		/// <summary>
		/// Creates the SQL script that is responsible for granting the specified login access to the specified database.
		/// </summary>
		/// <param name="databaseName">The name of the database that the login will be created for.</param>
		/// <param name="grantLoginName">Name of the SQL Server user that should have execute rights on the stored procedure.</param>
		/// <param name="path">Path where the script should be created.</param>
		/// <param name="createMultipleFiles">Indicates if the script should be created in its own file.</param>
		public static void CreateUserQueries(string databaseName, string grantLoginName, string path, bool createMultipleFiles)
		{
			if (grantLoginName.Length > 0)
			{
				string fileName;

				// Determine the file name to be used
				if (createMultipleFiles)
				{
					fileName = Path.Combine(path, "GrantUserPermissions.sql");
				}
				else
				{
					fileName = Path.Combine(path, "StoredProcedures.sql");
				}

				using (StreamWriter streamWriter = new StreamWriter(fileName, true))
				{
					streamWriter.Write(Utility.GetUserQueries(databaseName, grantLoginName));
				}
			}
		}


        /// <summary>
        /// Creates an insert stored procedure SQL script for the specified table
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="table">Instance of the Table class that represents the table this stored procedure will be created for.</param>
        /// <param name="grantLoginName">Name of the SQL Server user that should have execute rights on the stored procedure.</param>
        /// <param name="storedProcedurePrefix">Prefix to be appended to the name of the stored procedure.</param>
        /// <param name="path">Path where the stored procedure script should be created.</param>
        /// <param name="createMultipleFiles">Indicates the procedure(s) generated should be created in its own file.</param>
        public static void CreateSaveStoredProcedure(string databaseName, Table table, string grantLoginName, string storedProcedurePrefix, string path, bool createMultipleFiles)
        {
            // Create the stored procedure name
            string procedureName = Utility.FormatPascal(table.Name).Trim() + "Save";  //storedProcedurePrefix + table.Name + "Insert";
            string fileName;

            // Determine the file name to be used
            if (createMultipleFiles)
            {
                path = Path.Combine(path, "SaveSPs");
                fileName = Path.Combine(path, procedureName + ".sql");
            }
            else
            {
                fileName = Path.Combine(path, "StoredProcedures.sql");
            }

            using (StreamWriter streamWriter = new StreamWriter(fileName, true))
            {
                

               
                streamWriter.WriteLine("USE [" + databaseName + "]");
                streamWriter.WriteLine("GO");
                streamWriter.WriteLine("SET ANSI_NULLS ON");
                streamWriter.WriteLine("GO");
                streamWriter.WriteLine("SET QUOTED_IDENTIFIER ON");
                streamWriter.WriteLine("GO");
                streamWriter.WriteLine("CREATE PROCEDURE [dbo].[" + Utility.FormatPascal(table.Name) + "Save]");

                streamWriter.WriteLine("/*** ***************************************************************************************************** ****/");
                streamWriter.WriteLine("-- This procedure was Auto Genarated By A tool created by S.G. Asanga Chandrakumara");
                streamWriter.WriteLine("-- On 11:30 AM 7/14/2015");
                streamWriter.WriteLine("/*** ***************************************************************************************************** ****/");
                streamWriter.WriteLine("");

                // Create the parameter list
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    Column column = table.Columns[i];
                    if (column.IsIdentity == false && column.IsRowGuidCol == false)
                    {
                        streamWriter.Write("\t" + Utility.CreateParameterString(column, true));
                        streamWriter.Write(",");
                        if (i < (table.Columns.Count - 1))
                        {
                            
                        }
                        streamWriter.WriteLine();
                    }
                }
                streamWriter.WriteLine("\t@InsMode	int,");
                streamWriter.WriteLine("\t@RtnValue int Output");

                streamWriter.WriteLine("AS");
                streamWriter.WriteLine("BEGIN");
                streamWriter.WriteLine("Declare @DateNow datetime");
                streamWriter.WriteLine("BEGIN TRANSACTION T1");
                streamWriter.WriteLine("SET @DateNow=(select getdate())");
                streamWriter.WriteLine("--****************Insert New Record***********************");
                streamWriter.WriteLine("If @InsMode=1 ");
                streamWriter.WriteLine("   BEGIN");
                streamWriter.WriteLine("");
                streamWriter.WriteLine("insert into [" + table.Name + "]");
                streamWriter.WriteLine("(");

                // Create the parameter list
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    Column column = table.Columns[i];

                    // Ignore any identity columns
                    if (column.IsIdentity == false)
                    {
                        // Append the column name as a parameter of the insert statement
                        if (i < (table.Columns.Count - 1))
                        {
                            streamWriter.WriteLine("\t[" + column.Name + "],");
                        }
                        else
                        {
                            streamWriter.WriteLine("\t[" + column.Name + "]");
                        }
                    }
                }

                streamWriter.WriteLine(")");
                streamWriter.WriteLine("values");
                streamWriter.WriteLine("(");

                // Create the values list
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    Column column = table.Columns[i];

                    // Is the current column an identity column?
                    if (column.IsIdentity == false)
                    {
                        // Append the necessary line breaks and commas
                        if (i < (table.Columns.Count - 1))
                        {
                            streamWriter.WriteLine("\t@" + column.Name + ",");
                        }
                        else
                        {
                            streamWriter.WriteLine("\t@" + column.Name);
                        }
                    }
                }
                streamWriter.WriteLine(")");
                streamWriter.WriteLine("if @@error <> 0 goto Err_Desc");
                streamWriter.WriteLine("end");

                streamWriter.WriteLine("--****************Update the Record***********************");
                streamWriter.WriteLine("else if @Insmode=3");
                streamWriter.WriteLine("begin");

                streamWriter.WriteLine("update [" + table.Name + "]");
                streamWriter.Write("set");

                // Create the set statement
                bool firstLine = true;
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    Column column = (Column)table.Columns[i];

                    // Ignore Identity and RowGuidCol columns
                    if (table.PrimaryKeys.Contains(column) == false)
                    {
                        if (firstLine)
                        {
                            streamWriter.Write(" ");
                            firstLine = false;
                        }
                        else
                        {
                            streamWriter.Write("\t");
                        }

                        streamWriter.Write("[" + column.Name + "] = @" + column.Name);

                        if (i < (table.Columns.Count - 1))
                        {
                            streamWriter.Write(",");
                        }

                        streamWriter.WriteLine();
                    }
                }

                streamWriter.Write("where");

                // Create the where clause
                for (int i = 0; i < table.PrimaryKeys.Count; i++)
                {
                    Column column = table.PrimaryKeys[i];

                    if (i == 0)
                    {
                        streamWriter.Write(" [" + column.Name + "] = @" + column.Name);
                    }
                    else
                    {
                        streamWriter.Write("\tand [" + column.Name + "] = @" + column.Name);
                    }
                }
                streamWriter.WriteLine();

                streamWriter.WriteLine("end");
                streamWriter.WriteLine("----****************Delete the Record***********************");
                streamWriter.WriteLine("else");
                streamWriter.WriteLine("begin");

                streamWriter.WriteLine("delete from [" + table.Name + "]");
                streamWriter.Write("where");

                // Create the where clause
                for (int i = 0; i < table.PrimaryKeys.Count; i++)
                {
                    Column column = table.PrimaryKeys[i];

                    if (i == 0)
                    {
                        streamWriter.WriteLine(" [" + column.Name + "] = @" + column.Name);
                    }
                    else
                    {
                        streamWriter.WriteLine("\tand [" + column.Name + "] = @" + column.Name);
                    }
                }

                streamWriter.WriteLine("end");
                streamWriter.WriteLine("Set @RtnValue=1");
                streamWriter.WriteLine("COMMIT TRANSACTION T1");
                streamWriter.WriteLine("RETURN");
                streamWriter.WriteLine("Err_Desc:");
                streamWriter.WriteLine("ROLLBACK TRANSACTION T1");
                streamWriter.WriteLine("END");
               
                streamWriter.WriteLine("");
                streamWriter.WriteLine("");
                streamWriter.WriteLine("");
                streamWriter.WriteLine("");
                streamWriter.WriteLine("");
                streamWriter.WriteLine("");
                streamWriter.WriteLine("");
                streamWriter.WriteLine("");
                streamWriter.WriteLine("");
                streamWriter.WriteLine("");
                streamWriter.WriteLine("");
                streamWriter.WriteLine("");
                streamWriter.WriteLine("");
                streamWriter.WriteLine("");
                streamWriter.WriteLine("");
                streamWriter.WriteLine("");
                streamWriter.WriteLine("");
                streamWriter.WriteLine("");
                streamWriter.WriteLine("");
                streamWriter.WriteLine("");
                streamWriter.WriteLine("");
                streamWriter.WriteLine("");
                             

                //// Initialize all RowGuidCol columns
                //foreach (Column column in table.Columns)
                //{
                //    if (column.IsRowGuidCol)
                //    {
                //        streamWriter.WriteLine("set @" + column.Name + " = NewID()");
                //        streamWriter.WriteLine();
                //        break;
                //    }
                //}


            }
        }




        /// <summary>
        /// Creates an insert stored procedure SQL script for the specified table
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="table">Instance of the Table class that represents the table this stored procedure will be created for.</param>
        /// <param name="grantLoginName">Name of the SQL Server user that should have execute rights on the stored procedure.</param>
        /// <param name="storedProcedurePrefix">Prefix to be appended to the name of the stored procedure.</param>
        /// <param name="path">Path where the stored procedure script should be created.</param>
        /// <param name="createMultipleFiles">Indicates the procedure(s) generated should be created in its own file.</param>
        public static void CreateSaveStoredProcedureForSceinter(string databaseName, Table table, string grantLoginName, string storedProcedurePrefix, string path, bool createMultipleFiles)
        {
            // Create the stored procedure name
            string procedureName = Utility.FormatPascal(table.Name).Trim() + "Save";  //storedProcedurePrefix + table.Name + "Insert";
            string fileName;


            // Determine the file name to be used
            if (createMultipleFiles)
            {
                path = Path.Combine(path, "SaveSPScienterModel");
                fileName = Path.Combine(path, procedureName + ".sql");
            }
            else
            {
                fileName = Path.Combine(path, "StoredProcedures.sql");
            }

            using (StreamWriter streamWriter = new StreamWriter(fileName, true))
            {

                streamWriter.WriteLine("USE [" + databaseName + "]");
                streamWriter.WriteLine("GO");
                streamWriter.WriteLine("SET ANSI_NULLS ON");
                streamWriter.WriteLine("GO");
                streamWriter.WriteLine("SET QUOTED_IDENTIFIER ON");
                streamWriter.WriteLine("GO");

                streamWriter.WriteLine("/*** ***************************************************************************************************** ****/");
                streamWriter.WriteLine("-- This procedure was Auto Genarated By A tool created by S.G. Asanga Chandrakumara");
                streamWriter.WriteLine("-- ON" + DateTime.Now.ToString());
                streamWriter.WriteLine("/*** ***************************************************************************************************** ****/");
                streamWriter.WriteLine("");

                streamWriter.WriteLine("/*");
                streamWriter.WriteLine("-----------------------------------------------------");
                streamWriter.WriteLine("INPUTS");
                streamWriter.WriteLine("-----------------------------------------------------");
                streamWriter.WriteLine("- WHEN SAVE ADD ALL OTHER COLUMNS AS DEFAULT");
                streamWriter.WriteLine("- PASS @Action =1 TO INSERT OR UPDATE");
                streamWriter.WriteLine("- PASS @Action =2 TO DELETE");
                streamWriter.WriteLine("-----------------------------------------------------");
                streamWriter.WriteLine("OUTPUTS");
                streamWriter.WriteLine("-----------------------------------------------------");
                streamWriter.WriteLine("@status RETURNS 1 WHEN RECORD INSERTED AND NO ERRORS");
                streamWriter.WriteLine("@status RETURNS 2 WHEN RECORD UPDATED AND NO ERRORS");
                streamWriter.WriteLine("@status RETURNS 3 WHEN MORE THAN ONE RECORD DELETED AND NO ERRORS");
                streamWriter.WriteLine("@status RETURNS -3 WHEN DELETED SUCCESS BUT NO EFFECTED NO OF RECORDS");
                streamWriter.WriteLine("@status RETURNS -1 WHEN ANY KIND OF ERROR OCCURED");
                streamWriter.WriteLine("TO INSERT A RECORD PASS A NEGATIVE)VALUE TO @ID");
                streamWriter.WriteLine("*/");

                streamWriter.WriteLine("CREATE PROCEDURE[dbo].[SP_Save_"+ table.Name +"]");
                streamWriter.WriteLine(" --DECLARE PARAMETERS");
                streamWriter.WriteLine("");


                // Create the parameter list
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    Column column = table.Columns[i];
                    streamWriter.Write("\t" + Utility.CreateParameterString(column, true));
                    streamWriter.Write(",");
                    if (i < (table.Columns.Count - 1))
                    {

                    }
                    streamWriter.WriteLine();
                }


                streamWriter.WriteLine("\t@Action[int],");
                streamWriter.WriteLine("\t@status[int] output");
                streamWriter.WriteLine("");
                streamWriter.WriteLine("AS");
                streamWriter.WriteLine("\tDECLARE @statusIN[int]");
                streamWriter.WriteLine("\tSET @statusIN = 0;");
                streamWriter.WriteLine("BEGIN");
                streamWriter.WriteLine("BEGIN TRY");
                streamWriter.WriteLine("\tBEGIN TRANSACTION");
                streamWriter.WriteLine("IF @Action =1");
                streamWriter.WriteLine("\tBEGIN");
                streamWriter.Write("\t\tIF EXISTS(SELECT* FROM " + table.Name + " WHERE ");

                // Create the where clause
                for (int i = 0; i < table.PrimaryKeys.Count; i++)
                {
                    Column column = table.PrimaryKeys[i];
                    if (i == 0)
                    {
                        streamWriter.Write(" [" + column.Name + "] = @" + column.Name);
                    }
                    else
                    {
                        streamWriter.Write("\tand [" + column.Name + "] = @" + column.Name);
                    }
                }

                streamWriter.WriteLine(")");
                streamWriter.WriteLine("\t\tBEGIN");
                streamWriter.WriteLine("\t\t --UPDATE THE RECORD");
                streamWriter.WriteLine("\t\t UPDATE " + table.Name + " SET");

                //update
            

                bool firstLine = true;
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    Column column = (Column)table.Columns[i];
                    // Ignore Identity and RowGuidCol columns
                    if (table.PrimaryKeys.Contains(column) == false)
                    {
                        streamWriter.Write("\t\t");
                        streamWriter.Write("[" + column.Name + "] = @" + column.Name);
                        if (i < (table.Columns.Count - 1))
                        {
                            streamWriter.Write(",");
                        }
                        streamWriter.WriteLine();
                    }
                }
                streamWriter.Write("\t\tWHERE");
                // Create the where clause
                for (int i = 0; i < table.PrimaryKeys.Count; i++)
                {
                    Column column = table.PrimaryKeys[i];
                    if (i == 0)
                    {
                        streamWriter.Write(" [" + column.Name + "] = @" + column.Name);
                    }
                    else
                    {
                        streamWriter.Write("\tand [" + column.Name + "] = @" + column.Name);
                    }
                }

                streamWriter.WriteLine();
                streamWriter.WriteLine("\t\tSET @statusIN = 2");
                streamWriter.WriteLine("\tEND");
                streamWriter.WriteLine("\tELSE");
                streamWriter.WriteLine("\tBEGIN");
                streamWriter.WriteLine("\t--INSERT THE RECORD");
                streamWriter.WriteLine("\tINSERT INTO " + table.Name +"(");

                // Create the parameter list
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    Column column = table.Columns[i];

                    // Ignore any identity columns
                    if (column.IsIdentity == false)
                    {
                        // Append the column name as a parameter of the insert statement
                        if (i < (table.Columns.Count - 1))
                        {
                            streamWriter.WriteLine("\t\t[" + column.Name + "],");
                        }
                        else
                        {
                            streamWriter.WriteLine("\t\t[" + column.Name + "]");
                        }
                    }
                }

                streamWriter.WriteLine("\t\t)");
                streamWriter.WriteLine("\t\tVALUES");
                streamWriter.WriteLine("\t\t(");

                // Create the values list
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    Column column = table.Columns[i];

                    // Is the current column an identity column?
                    if (column.IsIdentity == false)
                    {
                        // Append the necessary line breaks and commas
                        if (i < (table.Columns.Count - 1))
                        {
                            streamWriter.WriteLine("\t\t@" + column.Name + ",");
                        }
                        else
                        {
                            streamWriter.WriteLine("\t\t@" + column.Name);
                        }
                    }
                }
                streamWriter.WriteLine("\t\t)");
                streamWriter.WriteLine("\t\tSELECT SCOPE_IDENTITY() AS ID");
                streamWriter.WriteLine("\t\tSET @statusIN = 1;");
                streamWriter.WriteLine("\tEND");
                streamWriter.WriteLine("\tEND --ACTION 2 ENDS");
                streamWriter.WriteLine("ELSE IF @Action = 2");
                streamWriter.WriteLine("\tBEGIN");

                streamWriter.Write("\t\tDELETE [" + table.Name + "]");
                streamWriter.Write(" WHERE");

                // Create the where clause
                for (int i = 0; i < table.PrimaryKeys.Count; i++)
                {
                    Column column = table.PrimaryKeys[i];

                    if (i == 0)
                    {
                        streamWriter.WriteLine(" [" + column.Name + "] = @" + column.Name);
                    }
                    else
                    {
                        streamWriter.WriteLine("\tand [" + column.Name + "] = @" + column.Name);
                    }
                }

                streamWriter.WriteLine("\tIF @@ROWCOUNT >0 ");
                streamWriter.WriteLine("\tBEGIN");
                streamWriter.WriteLine("\t\tSET @statusIN = 3;");
                streamWriter.WriteLine("\tEND");
                streamWriter.WriteLine("\tELSE");
                streamWriter.WriteLine("\tBEGIN");
                streamWriter.WriteLine("\t\tSET @statusIN = -3;");
                streamWriter.WriteLine("\tEND");
                streamWriter.WriteLine("END --ACTIION -3 ENDS");
                streamWriter.WriteLine("\tCOMMIT TRAN -- Transaction Success!");
                streamWriter.WriteLine("\tSET @status = @statusIN");
                streamWriter.WriteLine("END TRY");
                streamWriter.WriteLine("BEGIN CATCH");
                streamWriter.WriteLine("\tIF @@TRANCOUNT > 0");
                streamWriter.WriteLine("\tROLLBACK TRAN --RollBack in case of Error");
                streamWriter.WriteLine("\tSET @status = -1;");
                streamWriter.WriteLine("\t\tSELECT");
                streamWriter.WriteLine("\t\tERROR_NUMBER() AS ErrorNumber");
                streamWriter.WriteLine("\t\t,ERROR_SEVERITY() AS ErrorSeverity");
                streamWriter.WriteLine("\t\t,ERROR_STATE() AS ErrorState");
                streamWriter.WriteLine("\t\t,ERROR_PROCEDURE() AS ErrorProcedure");
                streamWriter.WriteLine("\t\t,ERROR_LINE() AS ErrorLine");
                streamWriter.WriteLine("\t,ERROR_MESSAGE() AS ErrorMessage");
                streamWriter.WriteLine("END CATCH");
                streamWriter.WriteLine("END--SP ENDS");


                }
            }


            /// <summary>
            /// Creates an insert stored procedure SQL script for the specified table
            /// </summary>
            /// <param name="databaseName">The name of the database.</param>
            /// <param name="table">Instance of the Table class that represents the table this stored procedure will be created for.</param>
            /// <param name="grantLoginName">Name of the SQL Server user that should have execute rights on the stored procedure.</param>
            /// <param name="storedProcedurePrefix">Prefix to be appended to the name of the stored procedure.</param>
            /// <param name="path">Path where the stored procedure script should be created.</param>
            /// <param name="createMultipleFiles">Indicates the procedure(s) generated should be created in its own file.</param>
            public static void CreateInsertStoredProcedure(string databaseName, Table table, string grantLoginName, string storedProcedurePrefix, string path, bool createMultipleFiles)
            {
                // Create the stored procedure name
                string procedureName = storedProcedurePrefix + table.Name + "Insert";
                string fileName;

                // Determine the file name to be used
                if (createMultipleFiles)
                {
                    fileName = Path.Combine(path, procedureName + ".sql");
                }
                else
                {
                    fileName = Path.Combine(path, "StoredProcedures.sql");
                }

                using (StreamWriter streamWriter = new StreamWriter(fileName, true))
                {
                    // Create the "use" statement or the seperator
                    if (createMultipleFiles)
                    {
                        CreateUseDatabaseStatement(databaseName, streamWriter);
                    }
                    else
                    {
                        streamWriter.WriteLine();
                        streamWriter.WriteLine("/******************************************************************************");
                        streamWriter.WriteLine("******************************************************************************/");
                }

				// Create the drop statment
				streamWriter.WriteLine("if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[" + procedureName + "]') and ObjectProperty(id, N'IsProcedure') = 1)");
				streamWriter.WriteLine("\tdrop procedure [dbo].[" + procedureName + "]");
				streamWriter.WriteLine("go");
				streamWriter.WriteLine();

				// Create the SQL for the stored procedure
				streamWriter.WriteLine("create procedure [dbo].[" + procedureName + "]");
				streamWriter.WriteLine("(");

				// Create the parameter list
				for (int i = 0; i < table.Columns.Count; i++)
				{
					Column column = table.Columns[i];
					if (column.IsIdentity == false && column.IsRowGuidCol == false)
					{
						streamWriter.Write("\t" + Utility.CreateParameterString(column, true));
						if (i < (table.Columns.Count - 1))
						{
							streamWriter.Write(",");
						}
						streamWriter.WriteLine();
					}
				}
				streamWriter.WriteLine(")");

				streamWriter.WriteLine();
				streamWriter.WriteLine("as");
				streamWriter.WriteLine();
				streamWriter.WriteLine("set nocount on");
				streamWriter.WriteLine();

				// Initialize all RowGuidCol columns
				foreach (Column column in table.Columns)
				{
					if (column.IsRowGuidCol)
					{
						streamWriter.WriteLine("set @" + column.Name + " = NewID()");
						streamWriter.WriteLine();
						break;
					}
				}

				streamWriter.WriteLine("insert into [" + table.Name + "]");
				streamWriter.WriteLine("(");

				// Create the parameter list
				for (int i = 0; i < table.Columns.Count; i++)
				{
					Column column = table.Columns[i];

					// Ignore any identity columns
					if (column.IsIdentity == false)
					{
						// Append the column name as a parameter of the insert statement
						if (i < (table.Columns.Count - 1))
						{
							streamWriter.WriteLine("\t[" + column.Name + "],");
						}
						else
						{
							streamWriter.WriteLine("\t[" + column.Name + "]");
						}
					}
				}

				streamWriter.WriteLine(")");
				streamWriter.WriteLine("values");
				streamWriter.WriteLine("(");

				// Create the values list
				for (int i = 0; i < table.Columns.Count; i++)
				{
					Column column = table.Columns[i];

					// Is the current column an identity column?
					if (column.IsIdentity == false)
					{
						// Append the necessary line breaks and commas
						if (i < (table.Columns.Count - 1))
						{
							streamWriter.WriteLine("\t@" + column.Name + ",");
						}
						else
						{
							streamWriter.WriteLine("\t@" + column.Name);
						}
					}
				}

				streamWriter.WriteLine(")");

				// Should we include a line for returning the identity?
				foreach (Column column in table.Columns)
				{
					// Is the current column an identity column?
					if (column.IsIdentity)
					{
						streamWriter.WriteLine();
						streamWriter.WriteLine("select scope_identity()");
						break;
					}
					else if (column.IsRowGuidCol)
					{
						streamWriter.WriteLine();
						streamWriter.WriteLine("Select @" + column.Name);
						break;
					}
				}

				streamWriter.WriteLine("go");

				// Create the grant statement, if a user was specified
				if (grantLoginName.Length > 0)
				{
					streamWriter.WriteLine();
					streamWriter.WriteLine("grant execute on [dbo].[" + procedureName + "] to [" + grantLoginName + "]");
					streamWriter.WriteLine("go");
				}
			}
		}

		/// <summary>
		/// Creates an update stored procedure SQL script for the specified table
		/// </summary>
		/// <param name="databaseName">The name of the database.</param>
		/// <param name="table">Instance of the Table class that represents the table this stored procedure will be created for.</param>
		/// <param name="grantLoginName">Name of the SQL Server user that should have execute rights on the stored procedure.</param>
		/// <param name="storedProcedurePrefix">Prefix to be appended to the name of the stored procedure.</param>
		/// <param name="path">Path where the stored procedure script should be created.</param>
		/// <param name="createMultipleFiles">Indicates the procedure(s) generated should be created in its own file.</param>
		public static void CreateUpdateStoredProcedure(string databaseName, Table table, string grantLoginName, string storedProcedurePrefix, string path, bool createMultipleFiles)
		{
			if (table.PrimaryKeys.Count > 0 && table.Columns.Count != table.PrimaryKeys.Count && table.Columns.Count != table.ForeignKeys.Count)
			{
				// Create the stored procedure name
				string procedureName = storedProcedurePrefix + table.Name + "Update";
				string fileName;

				// Determine the file name to be used
				if (createMultipleFiles)
				{
					fileName = Path.Combine(path, procedureName + ".sql");
				}
				else
				{
					fileName = Path.Combine(path, "StoredProcedures.sql");
				}

				using (StreamWriter streamWriter = new StreamWriter(fileName, true))
				{
				    // Create the "use" statement or the seperator
				    if (createMultipleFiles)
				    {
                        CreateUseDatabaseStatement(databaseName, streamWriter);
                    }
                    else
                    {
						streamWriter.WriteLine();
						streamWriter.WriteLine("/******************************************************************************");
						streamWriter.WriteLine("******************************************************************************/");
					}

					// Create the drop statment
					streamWriter.WriteLine("if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[" + procedureName + "]') and ObjectProperty(id, N'IsProcedure') = 1)");
					streamWriter.WriteLine("\tdrop procedure [dbo].[" + procedureName + "]");
					streamWriter.WriteLine("go");
					streamWriter.WriteLine();

					// Create the SQL for the stored procedure
					streamWriter.WriteLine("create procedure [dbo].[" + procedureName + "]");
					streamWriter.WriteLine("(");

					// Create the parameter list
					for (int i = 0; i < table.Columns.Count; i++)
					{
						Column column = table.Columns[i];
						
						if (i == 0)
						{
						
						}
						if (i < (table.Columns.Count - 1))
						{
							streamWriter.WriteLine("\t" + Utility.CreateParameterString(column, false) + ",");
						}
						else
						{
							streamWriter.WriteLine("\t" + Utility.CreateParameterString(column, false));
						}
					}
					streamWriter.WriteLine(")");

					streamWriter.WriteLine();
					streamWriter.WriteLine("as");
					streamWriter.WriteLine();
					streamWriter.WriteLine("set nocount on");
					streamWriter.WriteLine();
					streamWriter.WriteLine("update [" + table.Name + "]");
					streamWriter.Write("set");

					// Create the set statement
					bool firstLine = true;
					for (int i = 0; i < table.Columns.Count; i++)
					{
						Column column = (Column) table.Columns[i];

						// Ignore Identity and RowGuidCol columns
						if (table.PrimaryKeys.Contains(column) == false)
						{
							if (firstLine)
							{
								streamWriter.Write(" ");
								firstLine = false;
							}
							else
							{
								streamWriter.Write("\t");
							}

							streamWriter.Write("[" + column.Name + "] = @" + column.Name);

							if (i < (table.Columns.Count - 1))
							{
								streamWriter.Write(",");
							}
							
							streamWriter.WriteLine();
						}
					}

					streamWriter.Write("where");

					// Create the where clause
					for (int i = 0; i < table.PrimaryKeys.Count; i++)
					{
						Column column = table.PrimaryKeys[i];

						if (i == 0)
						{
							streamWriter.Write(" [" + column.Name + "] = @" + column.Name);
						}
						else
						{
							streamWriter.Write("\tand [" + column.Name + "] = @" + column.Name);
						}
					}
					streamWriter.WriteLine();

					streamWriter.WriteLine("go");

					// Create the grant statement, if a user was specified
					if (grantLoginName.Length > 0)
					{
						streamWriter.WriteLine();
						streamWriter.WriteLine("grant execute on [dbo].[" + procedureName + "] to [" + grantLoginName + "]");
						streamWriter.WriteLine("go");
					}
				}
			}
		}

		/// <summary>
		/// Creates an delete stored procedure SQL script for the specified table
		/// </summary>
		/// <param name="databaseName">The name of the database.</param>
		/// <param name="table">Instance of the Table class that represents the table this stored procedure will be created for.</param>
		/// <param name="grantLoginName">Name of the SQL Server user that should have execute rights on the stored procedure.</param>
		/// <param name="storedProcedurePrefix">Prefix to be appended to the name of the stored procedure.</param>
		/// <param name="path">Path where the stored procedure script should be created.</param>
		/// <param name="createMultipleFiles">Indicates the procedure(s) generated should be created in its own file.</param>
		public static void CreateDeleteStoredProcedure(string databaseName, Table table, string grantLoginName, string storedProcedurePrefix, string path, bool createMultipleFiles)
		{
			if (table.PrimaryKeys.Count > 0)
			{
				// Create the stored procedure name
				string procedureName = storedProcedurePrefix + table.Name + "Delete";
				string fileName;

				// Determine the file name to be used
				if (createMultipleFiles)
				{
					fileName = Path.Combine(path, procedureName + ".sql");
				}
				else
				{
					fileName = Path.Combine(path, "StoredProcedures.sql");
				}

				using (StreamWriter streamWriter = new StreamWriter(fileName, true))
				{
				    // Create the "use" statement or the seperator
				    if (createMultipleFiles)
				    {
                        CreateUseDatabaseStatement(databaseName, streamWriter);
                    }
                    else
                    {
						streamWriter.WriteLine();
						streamWriter.WriteLine("/******************************************************************************");
						streamWriter.WriteLine("******************************************************************************/");
					}

					// Create the drop statment
					streamWriter.WriteLine("if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[" + procedureName + "]') and ObjectProperty(id, N'IsProcedure') = 1)");
					streamWriter.WriteLine("\tdrop procedure [dbo].[" + procedureName + "]");
					streamWriter.WriteLine("go");
					streamWriter.WriteLine();

					// Create the SQL for the stored procedure
					streamWriter.WriteLine("create procedure [dbo].[" + procedureName + "]");
					streamWriter.WriteLine("(");

					// Create the parameter list
					for (int i = 0; i < table.PrimaryKeys.Count; i++)
					{
						Column column = table.PrimaryKeys[i];

						if (i < (table.PrimaryKeys.Count - 1))
						{
							streamWriter.WriteLine("\t" + Utility.CreateParameterString(column, false) + ",");
						}
						else
						{
							streamWriter.WriteLine("\t" + Utility.CreateParameterString(column, false));
						}
					}
					streamWriter.WriteLine(")");

					streamWriter.WriteLine();
					streamWriter.WriteLine("as");
					streamWriter.WriteLine();
					streamWriter.WriteLine("set nocount on");
					streamWriter.WriteLine();
					streamWriter.WriteLine("delete from [" + table.Name + "]");
					streamWriter.Write("where");

					// Create the where clause
					for (int i = 0; i < table.PrimaryKeys.Count; i++)
					{
						Column column = table.PrimaryKeys[i];

						if (i == 0)
						{
							streamWriter.WriteLine(" [" + column.Name + "] = @" + column.Name);
						}
						else
						{
							streamWriter.WriteLine("\tand [" + column.Name + "] = @" + column.Name);
						}
					}

					streamWriter.WriteLine("go");

					// Create the grant statement, if a user was specified
					if (grantLoginName.Length > 0)
					{
						streamWriter.WriteLine();
						streamWriter.WriteLine("grant execute on [dbo].[" + procedureName + "] to [" + grantLoginName + "]");
						streamWriter.WriteLine("go");
					}
				}
			}
		}

		/// <summary>
		/// Creates one or more delete stored procedures SQL script for the specified table and its foreign keys
		/// </summary>
		/// <param name="databaseName">The name of the database.</param>
		/// <param name="table">Instance of the Table class that represents the table this stored procedure will be created for.</param>
		/// <param name="grantLoginName">Name of the SQL Server user that should have execute rights on the stored procedure.</param>
		/// <param name="storedProcedurePrefix">Prefix to be appended to the name of the stored procedure.</param>
		/// <param name="path">Path where the stored procedure script should be created.</param>
		/// <param name="createMultipleFiles">Indicates the procedure(s) generated should be created in its own file.</param>
		public static void CreateDeleteAllByStoredProcedures(string databaseName, Table table, string grantLoginName, string storedProcedurePrefix, string path, bool createMultipleFiles)
		{
			// Create a stored procedure for each foreign key
			foreach (List<Column> compositeKeyList in table.ForeignKeys.Values)
			{
				// Create the stored procedure name
				StringBuilder stringBuilder = new StringBuilder(255);
				stringBuilder.Append(storedProcedurePrefix + table.Name + "DeleteAllBy");

				// Create the parameter list
				for (int i = 0; i < compositeKeyList.Count; i++)
				{
					Column column = compositeKeyList[i];
					if (i > 0)
					{
						stringBuilder.Append("_" + Utility.FormatPascal(column.Name));
					}
					else
					{
						stringBuilder.Append(Utility.FormatPascal(column.Name));
					}
				}

				string procedureName = stringBuilder.ToString();
				string fileName;

				// Determine the file name to be used
				if (createMultipleFiles)
				{
					fileName = Path.Combine(path, procedureName + ".sql");
				}
				else
				{
					fileName = Path.Combine(path, "StoredProcedures.sql");
				}

				using (StreamWriter streamWriter = new StreamWriter(fileName, true))
				{
				    // Create the "use" statement or the seperator
				    if (createMultipleFiles)
				    {
                        CreateUseDatabaseStatement(databaseName, streamWriter);
                    }
                    else
                    {
						streamWriter.WriteLine();
						streamWriter.WriteLine("/******************************************************************************");
						streamWriter.WriteLine("******************************************************************************/");
					}

					// Create the drop statment
					streamWriter.WriteLine("if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[" + procedureName + "]') and ObjectProperty(id, N'IsProcedure') = 1)");
					streamWriter.WriteLine("\tdrop procedure [dbo].[" + procedureName + "]");
					streamWriter.WriteLine("go");
					streamWriter.WriteLine();

					// Create the SQL for the stored procedure
					streamWriter.WriteLine("create procedure [dbo].[" + procedureName + "]");
					streamWriter.WriteLine("(");

					// Create the parameter list
					for (int i = 0; i < compositeKeyList.Count; i++)
					{
						Column column = compositeKeyList[i];

						if (i < (compositeKeyList.Count - 1))
						{
							streamWriter.WriteLine("\t" + Utility.CreateParameterString(column, false) + ",");
						}
						else
						{
							streamWriter.WriteLine("\t" + Utility.CreateParameterString(column, false));
						}
					}
					streamWriter.WriteLine(")");

					streamWriter.WriteLine();
					streamWriter.WriteLine("as");
					streamWriter.WriteLine();
					streamWriter.WriteLine("set nocount on");
					streamWriter.WriteLine();
					streamWriter.WriteLine("delete from [" + table.Name + "]");
					streamWriter.Write("where");

					// Create the where clause
					for (int i = 0; i < compositeKeyList.Count; i++)
					{
						Column column = compositeKeyList[i];

						if (i == 0)
						{
							streamWriter.WriteLine(" [" + column.Name + "] = @" + column.Name);
						}
						else
						{
							streamWriter.WriteLine("\tand [" + column.Name + "] = @" + column.Name);
						}
					}

					streamWriter.WriteLine("go");

					// Create the grant statement, if a user was specified
					if (grantLoginName.Length > 0)
					{
						streamWriter.WriteLine();
						streamWriter.WriteLine("grant execute on [dbo].[" + procedureName + "] to [" + grantLoginName + "]");
						streamWriter.WriteLine("go");
					}
				}
			}
		}

		/// <summary>
		/// Creates an select stored procedure SQL script for the specified table
		/// </summary>
		/// <param name="databaseName">The name of the database.</param>
		/// <param name="table">Instance of the Table class that represents the table this stored procedure will be created for.</param>
		/// <param name="grantLoginName">Name of the SQL Server user that should have execute rights on the stored procedure.</param>
		/// <param name="storedProcedurePrefix">Prefix to be appended to the name of the stored procedure.</param>
		/// <param name="path">Path where the stored procedure script should be created.</param>
		/// <param name="createMultipleFiles">Indicates the procedure(s) generated should be created in its own file.</param>
		public static void CreateSelectStoredProcedure(string databaseName, Table table, string grantLoginName, string storedProcedurePrefix, string path, bool createMultipleFiles)
		{
			if (table.PrimaryKeys.Count > 0 && table.ForeignKeys.Count != table.Columns.Count)
			{
				// Create the stored procedure name
				string procedureName = storedProcedurePrefix + table.Name + "Select";
				string fileName;

				// Determine the file name to be used
				if (createMultipleFiles)
				{
					fileName = Path.Combine(path, procedureName + ".sql");
				}
				else
				{
					fileName = Path.Combine(path, "StoredProcedures.sql");
				}

				using (StreamWriter streamWriter = new StreamWriter(fileName, true))
				{
				    // Create the "use" statement or the seperator
				    if (createMultipleFiles)
				    {
                        CreateUseDatabaseStatement(databaseName, streamWriter);
                    }
                    else
                    {
						streamWriter.WriteLine();
						streamWriter.WriteLine("/******************************************************************************");
						streamWriter.WriteLine("******************************************************************************/");
					}

					// Create the drop statment
					streamWriter.WriteLine("if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[" + procedureName + "]') and ObjectProperty(id, N'IsProcedure') = 1)");
					streamWriter.WriteLine("\tdrop procedure [dbo].[" + procedureName + "]");
					streamWriter.WriteLine("go");
					streamWriter.WriteLine();

					// Create the SQL for the stored procedure
					streamWriter.WriteLine("create procedure [dbo].[" + procedureName + "]");
					streamWriter.WriteLine("(");

					// Create the parameter list
					for (int i = 0; i < table.PrimaryKeys.Count; i++)
					{
						Column column = table.PrimaryKeys[i];

						if (i == (table.PrimaryKeys.Count - 1))
						{
							streamWriter.WriteLine("\t" + Utility.CreateParameterString(column, false));
						}
						else
						{
							streamWriter.WriteLine("\t" + Utility.CreateParameterString(column, false) + ",");
						}
					}

					streamWriter.WriteLine(")");

					streamWriter.WriteLine();
					streamWriter.WriteLine("as");
					streamWriter.WriteLine();
					streamWriter.WriteLine("set nocount on");
					streamWriter.WriteLine();
					streamWriter.Write("select");

					// Create the list of columns
					for (int i = 0; i < table.Columns.Count; i++)
					{
						Column column = table.Columns[i];

						if (i == 0)
						{
							streamWriter.Write(" ");
						}
						else
						{
							streamWriter.Write("\t");
						}

						streamWriter.Write("[" + column.Name + "]");

						if (i < (table.Columns.Count - 1))
						{
							streamWriter.Write(",");
						}

						streamWriter.WriteLine();
					}

					streamWriter.WriteLine("from [" + table.Name + "]");
					streamWriter.Write("where");

					// Create the where clause
					for (int i = 0; i < table.PrimaryKeys.Count; i++)
					{
						Column column = table.PrimaryKeys[i];

						if (i == 0)
						{
							streamWriter.WriteLine(" [" + column.Name + "] = @" + column.Name);
						}
						else
						{
							streamWriter.WriteLine("\tand [" + column.Name + "] = @" + column.Name);
						}
					}

					streamWriter.WriteLine("go");

					// Create the grant statement, if a user was specified
					if (grantLoginName.Length > 0)
					{
						streamWriter.WriteLine();
						streamWriter.WriteLine("grant execute on [dbo].[" + procedureName + "] to [" + grantLoginName + "]");
						streamWriter.WriteLine("go");
					}
				}
			}
		}

		/// <summary>
		/// Creates an select all stored procedure SQL script for the specified table
		/// </summary>
		/// <param name="databaseName">The name of the database.</param>
		/// <param name="table">Instance of the Table class that represents the table this stored procedure will be created for.</param>
		/// <param name="grantLoginName">Name of the SQL Server user that should have execute rights on the stored procedure.</param>
		/// <param name="storedProcedurePrefix">Prefix to be appended to the name of the stored procedure.</param>
		/// <param name="path">Path where the stored procedure script should be created.</param>
		/// <param name="createMultipleFiles">Indicates the procedure(s) generated should be created in its own file.</param>
		public static void CreateSelectAllStoredProcedure(string databaseName, Table table, string grantLoginName, string storedProcedurePrefix, string path, bool createMultipleFiles)
		{
			if (table.PrimaryKeys.Count > 0 && table.ForeignKeys.Count != table.Columns.Count)
			{
				// Create the stored procedure name
				string procedureName = storedProcedurePrefix + table.Name + "SelectAll";
				string fileName;

				// Determine the file name to be used
				if (createMultipleFiles)
				{
					fileName = Path.Combine(path, procedureName + ".sql");
				}
				else
				{
					fileName = Path.Combine(path, "StoredProcedures.sql");
				}

				using (StreamWriter streamWriter = new StreamWriter(fileName, true))
				{
				    // Create the "use" statement or the seperator
				    if (createMultipleFiles)
				    {
                        CreateUseDatabaseStatement(databaseName, streamWriter);
                    }
                    else
                    {
						streamWriter.WriteLine();
						streamWriter.WriteLine("/******************************************************************************");
						streamWriter.WriteLine("******************************************************************************/");
					}

					// Create the drop statment
					streamWriter.WriteLine("if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[" + procedureName + "]') and ObjectProperty(id, N'IsProcedure') = 1)");
					streamWriter.WriteLine("\tdrop procedure [dbo].[" + procedureName + "]");
					streamWriter.WriteLine("go");
					streamWriter.WriteLine();

					// Create the SQL for the stored procedure
					streamWriter.WriteLine("create procedure [dbo].[" + procedureName + "]");
					streamWriter.WriteLine();
					streamWriter.WriteLine("as");
					streamWriter.WriteLine();
					streamWriter.WriteLine("set nocount on");
					streamWriter.WriteLine();
					streamWriter.Write("select");

					// Create the list of columns
					for (int i = 0; i < table.Columns.Count; i++)
					{
						Column column = table.Columns[i];

						if (i == 0)
						{
							streamWriter.Write(" ");
						}
						else
						{
							streamWriter.Write("\t");
						}
						
						streamWriter.Write("[" + column.Name + "]");
						
						if (i < (table.Columns.Count - 1))
						{
							streamWriter.Write(",");
						}
						
						streamWriter.WriteLine();
					}

					streamWriter.WriteLine("from [" + table.Name + "]");

					streamWriter.WriteLine("go");

					// Create the grant statement, if a user was specified
					if (grantLoginName.Length > 0)
					{
						streamWriter.WriteLine();
						streamWriter.WriteLine("grant execute on [dbo].[" + procedureName + "] to [" + grantLoginName + "]");
						streamWriter.WriteLine("go");
					}
				}
			}
		}

		/// <summary>
		/// Creates one or more select stored procedures SQL script for the specified table and its foreign keys
		/// </summary>
		/// <param name="databaseName">The name of the database.</param>
		/// <param name="table">Instance of the Table class that represents the table this stored procedure will be created for.</param>
		/// <param name="grantLoginName">Name of the SQL Server user that should have execute rights on the stored procedure.</param>
		/// <param name="storedProcedurePrefix">Prefix to be appended to the name of the stored procedure.</param>
		/// <param name="path">Path where the stored procedure script should be created.</param>
		/// <param name="createMultipleFiles">Indicates the procedure(s) generated should be created in its own file.</param>
		public static void CreateSelectAllByStoredProcedures(string databaseName, Table table, string grantLoginName, string storedProcedurePrefix, string path, bool createMultipleFiles)
		{
			// Create a stored procedure for each foreign key
			foreach (List<Column> compositeKeyList in table.ForeignKeys.Values)
			{
				// Create the stored procedure name
				StringBuilder stringBuilder = new StringBuilder(255);
				stringBuilder.Append(storedProcedurePrefix + table.Name + "SelectAllBy");

				// Create the parameter list
				for (int i = 0; i < compositeKeyList.Count; i++)
				{
					Column column = compositeKeyList[i];
					if (i > 0)
					{
						stringBuilder.Append("_" + Utility.FormatPascal(column.Name));
					}
					else
					{
						stringBuilder.Append(Utility.FormatPascal(column.Name));
					}
				}

				string procedureName = stringBuilder.ToString();
				string fileName;

				// Determine the file name to be used
				if (createMultipleFiles)
				{
					fileName = Path.Combine(path, procedureName + ".sql");
				}
				else
				{
					fileName = Path.Combine(path, "StoredProcedures.sql");
				}

				using (StreamWriter streamWriter = new StreamWriter(fileName, true))
				{
				    // Create the "use" statement or the seperator
				    if (createMultipleFiles)
				    {
                        CreateUseDatabaseStatement(databaseName, streamWriter);
                    }
                    else
                    {
						streamWriter.WriteLine();
						streamWriter.WriteLine("/******************************************************************************");
						streamWriter.WriteLine("******************************************************************************/");
					}

					// Create the drop statment
					streamWriter.WriteLine("if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[" + procedureName + "]') and ObjectProperty(id, N'IsProcedure') = 1)");
					streamWriter.WriteLine("\tdrop procedure [dbo].[" + procedureName + "]");
					streamWriter.WriteLine("go");
					streamWriter.WriteLine();

					// Create the SQL for the stored procedure
					streamWriter.WriteLine("create procedure [dbo].[" + procedureName + "]");
					streamWriter.WriteLine("(");

					// Create the parameter list
					for (int i = 0; i < compositeKeyList.Count; i++)
					{
						Column column = compositeKeyList[i];

						if (i < (compositeKeyList.Count - 1))
						{
							streamWriter.WriteLine("\t" + Utility.CreateParameterString(column, false) + ",");
						}
						else
						{
							streamWriter.WriteLine("\t" + Utility.CreateParameterString(column, false));
						}
					}
					streamWriter.WriteLine(")");

					streamWriter.WriteLine();
					streamWriter.WriteLine("as");
					streamWriter.WriteLine();
					streamWriter.WriteLine("set nocount on");
					streamWriter.WriteLine();
					streamWriter.Write("select");

					// Create the list of columns
					for (int i = 0; i < table.Columns.Count; i++)
					{
						Column column = table.Columns[i];

						if (i == 0)
						{
							streamWriter.Write(" ");
						}
						else
						{
							streamWriter.Write("\t");
						}

						streamWriter.Write("[" + column.Name + "]");

						if (i < (table.Columns.Count - 1))
						{
							streamWriter.Write(",");
						}

						streamWriter.WriteLine();
					}

					streamWriter.WriteLine("from [" + table.Name + "]");
					streamWriter.Write("where");

					// Create the where clause
					for (int i = 0; i < compositeKeyList.Count; i++)
					{
						Column column = compositeKeyList[i];

						if (i == 0)
						{
							streamWriter.WriteLine(" [" + column.Name + "] = @" + column.Name);
						}
						else
						{
							streamWriter.WriteLine("\tand [" + column.Name + "] = @" + column.Name);
						}
					}

					streamWriter.WriteLine("go");

					// Create the grant statement, if a user was specified
					if (grantLoginName.Length > 0)
					{
						streamWriter.WriteLine();
						streamWriter.WriteLine("grant execute on [dbo].[" + procedureName + "] to [" + grantLoginName + "]");
						streamWriter.WriteLine("go");
					}
				}
			}
		}
	}
}

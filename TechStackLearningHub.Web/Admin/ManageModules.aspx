<%@ Page Title="Manage Modules" Language="C#" MasterPageFile="~/Masterpages/Admin.Master" AutoEventWireup="true" CodeBehind="ManageModules.aspx.cs" Inherits="TechStackLearningHub.Web.Admin.ManageModules" %>
<%@ Import Namespace="TechStackLearningHub.Web.Helpers" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row mb-3">
        <div class="col-md-5">
            <asp:Label runat="server" AssociatedControlID="ddlCourse" CssClass="form-label" Text="Course" />
            <asp:DropDownList ID="ddlCourse" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlCourse_SelectedIndexChanged" />
        </div>
    </div>

    <asp:Panel ID="pnlWorkspace" runat="server" Visible="false">
        <div class="row g-3">
            <div class="col-lg-8">
                <div class="card">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <span>Modules</span>
                    </div>
                    <div class="card-body p-0">
                        <asp:GridView ID="grdModules" runat="server" AutoGenerateColumns="false" CssClass="table table-hover mb-0"
                            DataKeyNames="ModuleID" OnRowCommand="grdModules_RowCommand" EmptyDataText="No modules in this course yet.">
                            <Columns>
                                <asp:TemplateField HeaderText="Order">
                                    <ItemTemplate>
                                        <%# Eval("ModuleOrder") %>
                        <%-- The arrows alone announce as "black up-pointing
                             triangle", so give each control a real name. --%>
                        <asp:LinkButton runat="server" Text="&#9650;" CommandName="MoveUp" CommandArgument='<%# Eval("ModuleID") %>' CssClass="btn btn-sm btn-link p-0 ms-2"
                            ToolTip='<%# "Move " + Eval("ModuleTitle") + " up" %>' />
                        <asp:LinkButton runat="server" Text="&#9660;" CommandName="MoveDown" CommandArgument='<%# Eval("ModuleID") %>' CssClass="btn btn-sm btn-link p-0"
                            ToolTip='<%# "Move " + Eval("ModuleTitle") + " down" %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="ModuleTitle" HeaderText="Title" />
                                <asp:TemplateField>
                                    <ItemTemplate>
                                        <asp:LinkButton runat="server" Text="Rename" CommandName="RenameModule" CommandArgument='<%# Eval("ModuleID") %>' CssClass="btn btn-sm btn-outline-primary" />
                                        <asp:LinkButton runat="server" Text="Lessons" CommandName="ManageLessons" CommandArgument='<%# Eval("ModuleID") %>' CssClass="btn btn-sm btn-outline-info" />
                                        <asp:LinkButton runat="server" Text="Delete" CommandName="DeleteModule" CommandArgument='<%# Eval("ModuleID") %>' CssClass="btn btn-sm btn-outline-danger"
                                            OnClientClick='<%# AdminUi.Confirm("Delete {0} and its lessons?", Eval("ModuleTitle")) %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
            </div>

            <div class="col-lg-4">
                <div class="card mb-3">
                    <div class="card-header">Add module</div>
                    <div class="card-body">
                        <asp:Label runat="server" AssociatedControlID="txtModuleTitle" CssClass="form-label" Text="New module title" />
                        <asp:TextBox ID="txtModuleTitle" runat="server" CssClass="form-control" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtModuleTitle" ErrorMessage="Title is required." CssClass="text-danger" Display="Dynamic" />
                        <div class="mt-3">
                            <asp:Button ID="btnAddModule" runat="server" Text="Add module" CssClass="btn btn-primary" OnClick="btnAddModule_Click" />
                        </div>
                    </div>
                </div>

                <asp:Panel ID="pnlRename" runat="server" CssClass="card" Visible="false">
                    <div class="card-header">
                        <h2 class="h6 card-title mb-0">Rename module</h2>
                    </div>
                    <div class="card-body">
                        <asp:HiddenField ID="hidRenameModuleId" runat="server" />
                        <div class="row g-3">
                            <div class="col-12">
                                <asp:TextBox ID="txtRenameTitle" runat="server" CssClass="form-control" />
                            </div>
                            <div class="col-12">
                                <asp:Button ID="btnSaveRename" runat="server" Text="Save" CssClass="btn btn-primary btn-sm" OnClick="btnSaveRename_Click" />
                                <asp:Button ID="btnCancelRename" runat="server" Text="Cancel" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnCancelRename_Click" />
                            </div>
                        </div>
                    </div>
                </asp:Panel>
            </div>
        </div>
    </asp:Panel>

    <asp:Label ID="lblMessage" runat="server" CssClass="text-danger d-block mt-3" Visible="false" role="alert" />
    <asp:Label ID="lblSuccess" runat="server" CssClass="text-success d-block mt-3" Visible="false" role="status" />
</asp:Content>

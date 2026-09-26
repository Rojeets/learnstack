<%@ Page Title="Manage Modules" Language="C#" MasterPageFile="~/Masterpages/Admin.Master" AutoEventWireup="true" CodeBehind="ManageModules.aspx.cs" Inherits="TechStackLearningHub.Web.Admin.ManageModules" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Manage Modules</h1>

    <div class="row mb-4">
        <div class="col-md-5">
            <label class="form-label" for="ddlCourse">Course</label>
            <asp:DropDownList ID="ddlCourse" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlCourse_SelectedIndexChanged" />
        </div>
    </div>

    <asp:Panel ID="pnlWorkspace" runat="server" Visible="false">
        <div class="row g-3 mb-3">
            <div class="col-md-5">
                <label class="form-label" for="txtModuleTitle">New module title</label>
                <asp:TextBox ID="txtModuleTitle" runat="server" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtModuleTitle" ErrorMessage="Title is required." CssClass="text-danger" Display="Dynamic" />
            </div>
            <div class="col-md-3 align-self-end">
                <asp:Button ID="btnAddModule" runat="server" Text="Add module" CssClass="btn btn-primary" OnClick="btnAddModule_Click" />
            </div>
        </div>

        <asp:Panel ID="pnlRename" runat="server" CssClass="card mb-3" Visible="false">
            <div class="card-body">
                <h6 class="card-title">Rename module</h6>
                <asp:HiddenField ID="hidRenameModuleId" runat="server" />
                <div class="row g-3">
                    <div class="col-md-6">
                        <asp:TextBox ID="txtRenameTitle" runat="server" CssClass="form-control" />
                    </div>
                    <div class="col-md-6">
                        <asp:Button ID="btnSaveRename" runat="server" Text="Save" CssClass="btn btn-primary btn-sm" OnClick="btnSaveRename_Click" />
                        <asp:Button ID="btnCancelRename" runat="server" Text="Cancel" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnCancelRename_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>

        <asp:GridView ID="grdModules" runat="server" AutoGenerateColumns="false" CssClass="table table-striped"
            DataKeyNames="ModuleID" OnRowCommand="grdModules_RowCommand" EmptyDataText="No modules in this course yet.">
            <Columns>
                <asp:TemplateField HeaderText="Order">
                    <ItemTemplate>
                        <%# Eval("ModuleOrder") %>
                        <asp:LinkButton runat="server" Text="&#9650;" CommandName="MoveUp" CommandArgument='<%# Eval("ModuleID") %>' CssClass="btn btn-sm btn-link p-0 ms-2" />
                        <asp:LinkButton runat="server" Text="&#9660;" CommandName="MoveDown" CommandArgument='<%# Eval("ModuleID") %>' CssClass="btn btn-sm btn-link p-0" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="ModuleTitle" HeaderText="Title" />
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:LinkButton runat="server" Text="Rename" CommandName="RenameModule" CommandArgument='<%# Eval("ModuleID") %>' CssClass="btn btn-sm btn-outline-primary" />
                        <asp:LinkButton runat="server" Text="Lessons" CommandName="ManageLessons" CommandArgument='<%# Eval("ModuleID") %>' CssClass="btn btn-sm btn-outline-info" />
                        <asp:LinkButton runat="server" Text="Delete" CommandName="DeleteModule" CommandArgument='<%# Eval("ModuleID") %>' CssClass="btn btn-sm btn-outline-danger"
                            OnClientClick="return confirm('Delete this module and its lessons?');" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </asp:Panel>

    <asp:Label ID="lblMessage" runat="server" CssClass="text-danger" Visible="false" />
</asp:Content>
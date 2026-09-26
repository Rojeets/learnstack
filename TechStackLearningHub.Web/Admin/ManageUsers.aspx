<%@ Page Title="Manage Users" Language="C#" MasterPageFile="~/Masterpages/Admin.Master" AutoEventWireup="true" CodeBehind="ManageUsers.aspx.cs" Inherits="TechStackLearningHub.Web.Admin.ManageUsers" %>
<%@ Import Namespace="TechStackLearningHub.Web.Helpers" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <asp:GridView ID="grdUsers" runat="server" AutoGenerateColumns="false" CssClass="table table-hover mb-0"
        DataKeyNames="UserID" OnRowCommand="grdUsers_RowCommand" EmptyDataText="No users registered yet." AllowPaging="true" PageSize="10" PagerStyle-CssClass="grid-pager">
        <Columns>
            <asp:BoundField DataField="Username" HeaderText="Username" />
            <asp:BoundField DataField="Email" HeaderText="Email" />
            <asp:TemplateField HeaderText="Status">
                <ItemTemplate>
                    <asp:Label runat="server" Text="Active" CssClass="badge text-bg-success" Visible='<%# (bool)Eval("IsActive") %>' />
                    <asp:Label runat="server" Text="Deactivated" CssClass="badge text-bg-danger" Visible='<%# !(bool)Eval("IsActive") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="RoleName" HeaderText="Role" />
            <asp:BoundField DataField="CreatedDate" HeaderText="Registered" DataFormatString="{0:d}" />
            <asp:TemplateField>
                <ItemTemplate>
                    <asp:LinkButton runat="server" Text="Make admin" CommandName="MakeAdmin" CommandArgument='<%# Eval("UserID") %>' CssClass="btn btn-sm btn-outline-secondary"
                        Visible='<%# Eval("RoleName").ToString() != "Admin" %>' />
                    <asp:LinkButton runat="server" Text="Make student" CommandName="MakeStudent" CommandArgument='<%# Eval("UserID") %>' CssClass="btn btn-sm btn-outline-info"
                        Visible='<%# Eval("RoleName").ToString() != "Student" %>' />
                    <asp:LinkButton runat="server" Text="Activate" CommandName="ToggleActive" CommandArgument='<%# Eval("UserID") %>' CssClass="btn btn-sm btn-outline-success"
                        Visible='<%# !(bool)Eval("IsActive") %>' />
                    <asp:LinkButton runat="server" Text="Deactivate" CommandName="ToggleActive" CommandArgument='<%# Eval("UserID") %>' CssClass="btn btn-sm btn-outline-danger"
                        Visible='<%# (bool)Eval("IsActive") %>'
                        OnClientClick='<%# AdminUi.Confirm("Deactivate {0}? They will no longer be able to log in.", Eval("Username")) %>' />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <PagerSettings Mode="Numeric" Position="Bottom" PreviousPageText="&#8592; Newer" NextPageText="Older &#8594;" />
    </asp:GridView>

    <asp:Label ID="lblMessage" runat="server" CssClass="text-danger" Visible="false" role="alert" />
    <asp:Label ID="lblSuccess" runat="server" CssClass="text-success" Visible="false" role="status" />
</asp:Content>
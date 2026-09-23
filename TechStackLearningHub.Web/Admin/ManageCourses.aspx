<%@ Page Title="Manage Courses" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ManageCourses.aspx.cs" Inherits="TechStackLearningHub.Web.Admin.ManageCourses" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Manage Courses</h1>

    <asp:Panel ID="pnlEditor" runat="server" CssClass="card mb-4">
        <div class="card-body">
            <h5 class="card-title" id="lblEditorHeading" runat="server">New course</h5>
            <asp:HiddenField ID="hidCourseId" runat="server" />
            <div class="row g-3">
                <div class="col-md-6">
                    <label class="form-label" for="txtCourseName">Course name</label>
                    <asp:TextBox ID="txtCourseName" runat="server" CssClass="form-control" />
                </div>
                <div class="col-md-6">
                    <label class="form-label" for="ddlTechStack">Tech stack</label>
                    <asp:DropDownList ID="ddlTechStack" runat="server" CssClass="form-select" />
                </div>
                <div class="col-12">
                    <label class="form-label" for="txtDescription">Description</label>
                    <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control" />
                </div>
            </div>
            <asp:Label ID="lblMessage" runat="server" CssClass="text-danger d-block mt-2" Visible="false" />
            <div class="mt-3">
                <asp:Button ID="btnSave" runat="server" Text="Save course" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:Button ID="btnCancelEdit" runat="server" Text="Cancel" CssClass="btn btn-outline-secondary" OnClick="btnCancelEdit_Click" Visible="false" />
            </div>
        </div>
    </asp:Panel>

    <asp:GridView ID="grdCourses" runat="server" AutoGenerateColumns="false" CssClass="table table-striped"
        DataKeyNames="CourseID" OnRowCommand="grdCourses_RowCommand" EmptyDataText="No courses yet.">
        <Columns>
            <asp:BoundField DataField="CourseName" HeaderText="Course" />
            <asp:BoundField DataField="TechStack" HeaderText="Tech stack" />
            <asp:BoundField DataField="Description" HeaderText="Description" />
            <asp:TemplateField HeaderText="Status">
                <ItemTemplate>
                    <asp:Label runat="server" Text="Published" CssClass="badge text-bg-success" Visible='<%# (bool)Eval("IsPublished") %>' />
                    <asp:Label runat="server" Text="Draft" CssClass="badge text-bg-secondary" Visible='<%# !(bool)Eval("IsPublished") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField>
                <ItemTemplate>
                    <asp:LinkButton runat="server" Text="Edit" CommandName="EditCourse" CommandArgument='<%# Eval("CourseID") %>' CssClass="btn btn-sm btn-outline-primary" />
                    <asp:LinkButton runat="server" Text="Publish" CommandName="TogglePublish" CommandArgument='<%# Eval("CourseID") %>' CssClass="btn btn-sm btn-outline-success"
                        Visible='<%# !(bool)Eval("IsPublished") %>' />
                    <asp:LinkButton runat="server" Text="Unpublish" CommandName="TogglePublish" CommandArgument='<%# Eval("CourseID") %>' CssClass="btn btn-sm btn-outline-warning"
                        Visible='<%# (bool)Eval("IsPublished") %>' />
                    <asp:LinkButton runat="server" Text="Delete" CommandName="DeleteCourse" CommandArgument='<%# Eval("CourseID") %>' CssClass="btn btn-sm btn-outline-danger"
                        OnClientClick="return confirm('Delete this course and everything under it?');" />
                    <asp:LinkButton runat="server" Text="Modules" CommandName="ManageModules" CommandArgument='<%# Eval("CourseID") %>' CssClass="btn btn-sm btn-outline-info" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
</asp:Content>
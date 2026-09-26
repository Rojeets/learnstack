<%@ Page Title="Manage Courses" Language="C#" MasterPageFile="~/Masterpages/Admin.Master" AutoEventWireup="true" CodeBehind="ManageCourses.aspx.cs" Inherits="TechStackLearningHub.Web.Admin.ManageCourses" %>
<%@ Import Namespace="TechStackLearningHub.Web.Helpers" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row g-3">
        <div class="col-lg-8">
            <div class="card">
                <div class="card-header d-flex justify-content-between align-items-center">
                    <span>Courses</span>
                </div>
                <div class="card-body p-0">
                    <asp:GridView ID="grdCourses" runat="server" AutoGenerateColumns="false" CssClass="table table-hover mb-0"
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
                                    <asp:LinkButton runat="server" Text="Modules" CommandName="ManageModules" CommandArgument='<%# Eval("CourseID") %>' CssClass="btn btn-sm btn-outline-info" />
                                    <asp:LinkButton runat="server" Text="Delete" CommandName="DeleteCourse" CommandArgument='<%# Eval("CourseID") %>' CssClass="btn btn-sm btn-outline-danger"
                                        OnClientClick='<%# AdminUi.Confirm("Delete {0} and everything under it? This cannot be undone.", Eval("CourseName")) %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>

        <div class="col-lg-4">
            <asp:Panel ID="pnlEditor" runat="server" CssClass="card">
                <div class="card-header">
                    <h2 class="h5 card-title mb-0" id="lblEditorHeading" runat="server">New course</h2>
                </div>
                <div class="card-body">
                    <asp:HiddenField ID="hidCourseId" runat="server" />
                    <div class="row g-3">
                        <div class="col-12">
                            <asp:Label runat="server" AssociatedControlID="txtCourseName" CssClass="form-label" Text="Course name" />
                            <asp:TextBox ID="txtCourseName" runat="server" CssClass="form-control" />
                        </div>
                        <div class="col-12">
                            <asp:Label runat="server" AssociatedControlID="ddlTechStack" CssClass="form-label" Text="Tech stack" />
                            <asp:DropDownList ID="ddlTechStack" runat="server" CssClass="form-select" />
                        </div>
                        <div class="col-12">
                            <asp:Label runat="server" AssociatedControlID="txtDescription" CssClass="form-label" Text="Description" />
                            <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control" />
                        </div>
                    </div>
                    <asp:Label ID="lblMessage" runat="server" CssClass="text-danger d-block mt-2" Visible="false" role="alert" />
                    <asp:Label ID="lblSuccess" runat="server" CssClass="text-success d-block mt-2" Visible="false" role="status" />
                    <div class="mt-3">
                        <asp:Button ID="btnSave" runat="server" Text="Save course" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:Button ID="btnCancelEdit" runat="server" Text="Cancel" CssClass="btn btn-outline-secondary" OnClick="btnCancelEdit_Click" Visible="false" />
                    </div>
                </div>
            </asp:Panel>
        </div>
    </div>
</asp:Content>

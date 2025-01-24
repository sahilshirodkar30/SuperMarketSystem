import React, { useState, useEffect } from "react";
import "./Employees.css"; // Add custom CSS for styling
import api from "../pages/api"; // Replace with your actual API instance path

const Employees = () => {
    const [employees, setEmployees] = useState<
        { employeeId: number; firstName: string; lastName: string; role: string; salary: number; imageUrl: string | null }[]
    >([]);
    const [filteredEmployees, setFilteredEmployees] = useState(employees);
    const [form, setForm] = useState({
        firstName: "",
        lastName: "",
        role: "",
        salary: "",
        image: null,
    });
    const [searchQuery, setSearchQuery] = useState("");
    const [popupVisible, setPopupVisible] = useState(false);
    const [actionType, setActionType] = useState("add");
    const [selectedEmployee, setSelectedEmployee] = useState<
        { employeeId: number; firstName: string; lastName: string; role: string; salary: number; imageUrl: string | null } | null
    >(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");
    const [pageNumber, setPageNumber] = useState(1);
    const [pageSize, setPageSize] = useState(5);
    const [totalPages, setTotalPages] = useState(1);

    const fetchEmployees = async () => {
        try {
            setLoading(true);
            const response = await api.get("Employees", {
                params: { pageNumber, pageSize },
            });

            if (response.data) {
                setEmployees(response.data.data);
                setFilteredEmployees(response.data.data);
                setTotalPages(response.data.totalPages);
            }
        } catch (err) {
            console.error(err);
            setError("Failed to fetch employees.");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchEmployees();
    }, [pageNumber, pageSize]);

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setForm((prev) => ({ ...prev, [name]: value }));
    };

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            setForm((prev) => ({ ...prev, image: e.target.files[0] }));
        }
    };

    const handleSearch = (e: React.ChangeEvent<HTMLInputElement>) => {
        const query = e.target.value.toLowerCase();
        setSearchQuery(query);
        if (query.trim() === "") {
            setFilteredEmployees(employees);
        } else {
            const filtered = employees.filter(
                (emp) =>
                    emp.firstName.toLowerCase().includes(query) ||
                    emp.lastName.toLowerCase().includes(query) ||
                    emp.role.toLowerCase().includes(query)
            );
            setFilteredEmployees(filtered);
        }
    };

    const handleSubmit = async () => {
        setLoading(true);
        setError("");

        const formData = new FormData();
        formData.append("firstName", form.firstName);
        formData.append("lastName", form.lastName);
        formData.append("role", form.role);
        formData.append("salary", form.salary);
        if (form.image) formData.append("image", form.image);

        try {
            if (actionType === "update" && selectedEmployee) {
                await api.put(`Employees/${selectedEmployee.employeeId}`, formData);
            } else if (actionType === "add") {
                await api.post("Employees", formData);
            }
            fetchEmployees();
            closePopup();
        } catch (err) {
            console.error(err);
            setError("Failed to save employee.");
        } finally {
            setLoading(false);
        }
    };

    const handleDelete = async () => {
        if (!selectedEmployee) return;
        try {
            await api.delete(`Employees/${selectedEmployee.employeeId}`);
            fetchEmployees();
            closePopup();
        } catch (err) {
            console.error(err);
            setError("Failed to delete employee.");
        }
    };

    const openPopup = (type: string, employee: any) => {
        setActionType(type);
        setSelectedEmployee(employee || null);
        setForm({
            firstName: employee?.firstName || "",
            lastName: employee?.lastName || "",
            role: employee?.role || "",
            salary: employee?.salary || "",
            image: null,
        });
        setPopupVisible(true);
    };

    const closePopup = () => {
        setPopupVisible(false);
        setSelectedEmployee(null);
        setForm({
            firstName: "",
            lastName: "",
            role: "",
            salary: "",
            image: null,
        });
    };

    const handlePageChange = (newPage: number) => {
        if (newPage >= 1 && newPage <= totalPages) {
            setPageNumber(newPage);
        }
    };

    return (
        <div className="employees-container">
            <h1>Employee Management</h1>
            {error && <div className="error-message">{error}</div>}
            {loading && <div className="loading-message">Loading employees...</div>}

            <div className="header-actions">
                <button className="add-button" onClick={() => openPopup("add", null)}>
                    Add Employee
                </button>
                <input
                    type="text"
                    className="search-input"
                    placeholder="Search employees..."
                    value={searchQuery}
                    onChange={handleSearch}
                />
            </div>

            <table className="employees-table">
                <thead>
                    <tr>
                        <th>First Name</th>
                        <th>Last Name</th>
                        <th>Role</th>
                        <th>Salary</th>
                        <th>Image</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    {filteredEmployees.length > 0 ? (
                        filteredEmployees.map((emp) => (
                            <tr key={emp.employeeId}>
                                <td>{emp.firstName}</td>
                                <td>{emp.lastName}</td>
                                <td>{emp.role}</td>
                                <td>${emp.salary}</td>
                                <td>
                                    {emp.imageUrl && (
                                        <img
                                            src={emp.imageUrl}
                                            alt="Employee"
                                            className="employee-image"
                                        />
                                    )}
                                </td>
                                <td>
                                    <button
                                        className="update-button"
                                        onClick={() => openPopup("update", emp)}
                                    >
                                        Update
                                    </button>
                                    <button
                                        className="delete-button"
                                        onClick={() => openPopup("delete", emp)}
                                    >
                                        Delete
                                    </button>
                                </td>
                            </tr>
                        ))
                    ) : (
                        <tr>
                            <td colSpan={6}>No employees found.</td>
                        </tr>
                    )}
                </tbody>
            </table>

            <div className="pagination">
                <button
                    className="pagination-button"
                    onClick={() => handlePageChange(pageNumber - 1)}
                    disabled={pageNumber === 1}
                >
                    Previous
                </button>
                <span>
                    Page {pageNumber} of {totalPages}
                </span>
                <button
                    className="pagination-button"
                    onClick={() => handlePageChange(pageNumber + 1)}
                    disabled={pageNumber === totalPages}
                >
                    Next
                </button>
            </div>

            {popupVisible && (
                <div className="popup">
                    <div className="popup-content">
                        {actionType === "delete" ? (
                            <p>
                                Are you sure you want to delete {selectedEmployee?.firstName}?
                            </p>
                        ) : (
                            <form
                                onSubmit={(e) => {
                                    e.preventDefault();
                                    handleSubmit();
                                }}
                            >
                                <div className="form-group">
                                    <label>First Name</label>
                                    <input
                                        type="text"
                                        name="firstName"
                                        value={form.firstName}
                                        onChange={handleInputChange}
                                        required
                                    />
                                </div>
                                <div className="form-group">
                                    <label>Last Name</label>
                                    <input
                                        type="text"
                                        name="lastName"
                                        value={form.lastName}
                                        onChange={handleInputChange}
                                        required
                                    />
                                </div>
                                <div className="form-group">
                                    <label>Role</label>
                                    <input
                                        type="text"
                                        name="role"
                                        value={form.role}
                                        onChange={handleInputChange}
                                        required
                                    />
                                </div>
                                <div className="form-group">
                                    <label>Salary</label>
                                    <input
                                        type="number"
                                        name="salary"
                                        value={form.salary}
                                        onChange={handleInputChange}
                                        required
                                    />
                                </div>
                                <div className="form-group">
                                    <label>Image</label>
                                    <input
                                        type="file"
                                        name="image"
                                        onChange={handleFileChange}
                                    />
                                </div>
                                <button className="submit-button" type="submit" disabled={loading}>
                                    {actionType === "update" ? "Update" : "Add"}
                                </button>
                            </form>
                        )}
                        <button className="cancel-button" onClick={closePopup}>
                            Cancel
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
};

export default Employees;

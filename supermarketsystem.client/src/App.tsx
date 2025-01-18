import React, { useState } from 'react';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import SignupPage from './pages/SignupPage';
import HomePage from './pages/HomePage';
import ProductAll from './pages/ProductAll'
function App() {
    //const [isAuthenticated, setIsAuthenticated] = useState(!!localStorage.getItem('token'));

    //const handleLogout = () => {
    //    setIsAuthenticated(false);
    //};

    return (
        <Router>
          {/*<Navbar isAuthenticated={isAuthenticated} onLogout={handleLogout} />*/}
            <Routes>
                <Route path="/" element={<HomePage />} />
                <Route path="/signup" element={<SignupPage />} />
                <Route path="/ProductAll" element={<ProductAll />} />
            </Routes>
        </Router>
    );

   
}

export default App;
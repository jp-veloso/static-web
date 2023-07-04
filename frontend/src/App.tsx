import { useState } from "react"
import { BrowserRouter, Route, RouterProvider, Routes } from "react-router-dom"
import { AsideContext } from "./core/script"
import Auth from "./pages/Auth"
import Client from "./pages/Client"
import Details from "./pages/Client/components/Details"
import List from "./pages/Client/components/List"
import Home from "./pages/Home"
import { GoogleOAuthProvider } from "@react-oauth/google"
import PrivateRoute from "./core/PrivateRoute"
import Proposal from "./pages/Proposal"
import CompanyList from "./pages/Company/components/CompanyList"
import Company from "./pages/Company"
import CompanyCreate from "./pages/Company/components/CompanyCreate"
import Analysis from "./pages/Analysis"

const App = () => {
    const [open, setOpen] = useState(true)

    return (
        <GoogleOAuthProvider clientId={import.meta.env.VITE_GOOGLE_URL} key={import.meta.env.VITE_GOOGLE_KEY}>
            <AsideContext.Provider value={{ isOpen: open, setContext: setOpen }}>
                <Routes>
                    <Route path='/login' element={<Auth />} />
                    <Route element={<PrivateRoute allowedRoles={["contributor"]} />} >
                        <Route path='/' element={<Home />} />
                        <Route path='/clients' element={<Client />}>
                            <Route path='' element={<List />}></Route>
                            <Route path=':id' element={<Details />}></Route>
                            <Route path=':id/proposals' element={<Proposal />}></Route>
                        </Route>
                        <Route path='/companies' element={<Company />}>
                            <Route path='' element={<CompanyList />}></Route>
                            <Route element={<PrivateRoute allowedRoles={["admin"]} />}>
                                <Route path=':id/public' element={<CompanyCreate/>}></Route>
                                <Route path=':id/private' element={<CompanyCreate/>}></Route>
                            </Route>
                        </Route>
                    </Route>
                    <Route element={<PrivateRoute allowedRoles={["admin"]} />}>
                        <Route path='/analysis' element={<Analysis/>} />
                    </Route>
                </Routes>
            </AsideContext.Provider>
        </GoogleOAuthProvider>
    )
}

export default App
import { Outlet } from "react-router-dom";
import Aside from "../../core/Aside";
import Header from "../../core/Header";
import "./style.css"

const Company = () => {

    return <section className="flex w-full h-full">
        <Aside/>

        <div className="w-full bg-neutral-100">
            <Header/>
            <Outlet/>
        </div>
    </section>

}

export default Company;
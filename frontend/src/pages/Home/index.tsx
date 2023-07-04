import Aside from "../../core/Aside";
import Header from "../../core/Header";
import _boy from '../../assets/boy.png';
import _search from '../../assets/search.png';
import { useNavigate } from 'react-router-dom';
import Goal from "./components/Goal";
import { useEffect, useState } from "react";
import { Kpis, User } from "../../core/types";
import { getUserDetailed, makePrivateRequest } from "../../core/script";

const Home = () => {
    const navigate = useNavigate();
    const [user, setUser] = useState<User>()
    const [kpis, setKpis] = useState<Kpis>()

    useEffect(() => {
        setUser(getUserDetailed())

        makePrivateRequest({ url: "/auth/me" })
        .then(r => {
            setKpis(r.data.data.kpis)
        })
    },[])

    return <section className="flex w-full h-full">
        <Aside />
        <div className="w-full bg-neutral-100">
            <Header />
            <div className="h-[calc(100vh-4rem)] overflow-y-scroll scrollbar">
                <div className="flex bg-white shadow mt-9 mx-6 rounded-xl p-6 justify-between">
                    <div className="basis-2/3">
                        <p className="text-neutral-300 text-base">Portal Umbrella</p>
                        <h2 className="font-bold text-neutral-900 text-4xl my-4">Olá, {user?.name}</h2>
                        <p className="text-neutral-400 text-2xl font-medium">Seja bem-vindo ao portal do colaborador, esse portal servirá para você consultar os clientes e as seguradoras.</p>
                        <button className='auth-button rounded-xl p-4 text-white flex mt-3' onClick={() => navigate("/clients")}>
                            <img className='mr-2 mt-0.5' src={_search}></img>
                            <span>Pesquisar por cliente</span>
                        </button>
                    </div>
                    <img src={_boy}/>
                </div>
                {kpis && (<Goal kpis={kpis}/>)}
            </div>
        </div>
    </section>

}

export default Home;
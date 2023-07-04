import { useEffect, useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom'
import { User } from '../types';
import { getUserDetailed } from '../script';

const Header = () => {
    const location = useLocation();
    const [user, setUser] = useState<User>()

    const renderHeader = () => {
        let name = location.pathname;

        if(name.startsWith("/clients")){
            return <ClientHeader path={location.pathname} />
        }

        if(name.startsWith("/companies")){
            return <CompanyHeader path={location.pathname} />
        }

        console.log(name)
        return <HomeHeader/>

    }

    useEffect(() => {
        setUser(getUserDetailed())
    }, [])

    return <div className="h-16 w-full  bg-neutral-50 flex">
        {renderHeader()}
        <div className="basis-1/3 flex items-center justify-end space-x-4 px-8">
            {user?.picture ? <img className="h-12 rounded-full" src={user?.picture} referrerPolicy="no-referrer" alt='profile-picture' /> : <div className='h-12 w-12 flex items-center font-bold justify-center rounded-full bg-violet-100'>{user?.name.charAt(0).toUpperCase()}</div>}
            <div>
                <h4 className="font-semibold text-lg text-neutral-900 capitalize tracking-wide">{user?.name}</h4>
                <span className="text-sm tracking-wide flex items-center space-x-1 text-neutral-900">{user?.department === 'none' ? 'Sem departamento' : user?.department}</span>
            </div>
        </div>
    </div>
}

type Props = {
    path: string;
}

const HomeHeader = () => {
    return <ul className="flex items-center p-6 basis-2/3 text-base">
        <li>
            <a className="flex items-center space-x-3 text-neutral-900 font-bold mr-3">
                <span>Página principal</span>
            </a>
        </li>
    </ul>
}

const CompanyHeader = ({ path }: Props) => {
    const navigate = useNavigate();

    return <ul className="flex items-center p-6 basis-2/3 text-base">
        <li>
            <a className="flex items-center space-x-3 text-neutral-900 font-bold mr-3 cursor-pointer" onClick={() => navigate('/')}>
                <span>Página principal</span>
            </a>
        </li>
        <li>
            <a className={`flex items-center space-x-3 font-bold mr-3 cursor-pointer ${path.length < 12 ? 'text-violet-300' : 'text-neutral-900'}`} onClick={() => navigate('/companies')}>
                <span>
                    <svg width="16" height="16" viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
                        <path d="M6 3.33329L10.6667 7.99996L6 12.6666" stroke="#C2C2C2" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
                    </svg>
                </span>
                <span>Seguradoras</span>
            </a>
        </li>
        <li>
            <a className={`flex items-center space-x-3 font-bold mr-3  ${path.length >= 12 ? 'cursor-default text-violet-300' : 'hidden'}`}>
                <span>
                    <svg width="16" height="16" viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
                        <path d="M6 3.33329L10.6667 7.99996L6 12.6666" stroke="#C2C2C2" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
                    </svg>
                </span>
                <span>Cadastro</span>
            </a>
        </li>
        
    </ul>

}

const ClientHeader = ({ path }: Props) => {
    const navigate = useNavigate();

    return <ul className="flex items-center p-6 basis-2/3 text-base">
        <li>
            <a className="flex items-center space-x-3 text-neutral-900 font-bold mr-3 cursor-pointer" onClick={() => navigate('/')}>
                <span>Página principal</span>
            </a>
        </li>
        <li>
            <a className={`flex items-center space-x-3 font-bold mr-3 cursor-pointer ${path.length <= 9 ? 'text-violet-300' : 'text-neutral-900'}`} onClick={() => navigate('/clients')}>
                <span>
                    <svg width="16" height="16" viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
                        <path d="M6 3.33329L10.6667 7.99996L6 12.6666" stroke="#C2C2C2" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
                    </svg>
                </span>
                <span>Clientes</span>
            </a>
        </li>
        <li>
            <a onClick={() => {if(path.length > 13) navigate(-1)}} className={`flex items-center space-x-3 font-bold mr-3  ${path.length < 9 ? 'hidden' : ''} ${path.length > 9 && path.length <= 13 ? 'cursor-default text-violet-300' : 'cursor-pointer text-neutral-900'}`}>
                <span>
                    <svg width="16" height="16" viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
                        <path d="M6 3.33329L10.6667 7.99996L6 12.6666" stroke="#C2C2C2" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
                    </svg>
                </span>
                <span>Informação do cliente</span>
            </a>
        </li>
        <li>
            <a className={`flex items-center space-x-3 font-bold mr-3 cursor-default ${path.length > 13 ? 'text-violet-300' : 'hidden'}`}>
                <span>
                    <svg width="16" height="16" viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
                        <path d="M6 3.33329L10.6667 7.99996L6 12.6666" stroke="#C2C2C2" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
                    </svg>
                </span>
                <span>Cálculo de propostas</span>
            </a>
        </li>
    </ul>
}

export default Header;
import { useContext } from 'react';
import { NavLink } from 'react-router-dom';
import _logo from '../../assets/_logo2.svg'
import _minLogo from '../../assets/_minLogo.png'
import { AsideContext, isAllowedByRole, isAuthenticated, logout } from '../script';

const Aside = () => {
    const { isOpen, setContext } = useContext(AsideContext)

    return <aside className={`${isOpen ? 'w-72' : 'w-24'} bg-neutral-50 shadow-lg h-screen flex flex-col items-center transition-all relative`}>
        <div className="space-y-1 absolute right-1 top-2.5 cursor-pointer" onClick={() => setContext(!isOpen)}>
            <span className="block w-6 h-0.5 bg-purple-600"></span>
            <span className="block w-6 h-0.5 bg-purple-600"></span>
            <span className="block w-6 h-0.5 bg-purple-600"></span>
        </div>

        <img src={isOpen ? _logo : _minLogo} alt='umbrella' className={`${isOpen ? 'w-36' : 'w-14'} my-6`} />
        <ul className='space-y-2 text-base p-4 w-full basis-full'>
            <li>
                <NavLink to={"/"} className={event => `flex ${isOpen ? '' : 'justify-center'} items-center space-x-3 text-neutral-900 p-2 rounded-md font-medium hover:bg-violet-100 focus:shadow-outline cursor-pointer mb-2 ${event.isActive ? 'bg-violet-100' : ''}`}>
                    <span className="text-gray-600">
                        <svg width="32" height="32" viewBox="0 0 32 32" fill="none" xmlns="http://www.w3.org/2000/svg">
                            <path d="M4 16L6.66667 13.3333M6.66667 13.3333L16 4L25.3333 13.3333M6.66667 13.3333V26.6667C6.66667 27.403 7.26362 28 8 28H12M25.3333 13.3333L28 16M25.3333 13.3333V26.6667C25.3333 27.403 24.7364 28 24 28H20M12 28C12.7364 28 13.3333 27.403 13.3333 26.6667V21.3333C13.3333 20.597 13.9303 20 14.6667 20H17.3333C18.0697 20 18.6667 20.597 18.6667 21.3333V26.6667C18.6667 27.403 19.2636 28 20 28M12 28H20" stroke="#323232" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
                        </svg>
                    </span>
                    {isOpen && (<span>Home</span>)}
                </NavLink>
            </li>
            <li>
                <NavLink to={"/clients"} className={event => `flex ${isOpen ? '' : 'justify-center'} items-center space-x-3 text-neutral-900 p-2 rounded-md font-medium hover:bg-violet-100 focus:shadow-outline cursor-pointer mb-2 ${event.isActive ? 'bg-violet-100' : ''}`}>
                    <span className="text-gray-600">
                        <svg width="32" height="32" viewBox="0 0 32 32" fill="none" xmlns="http://www.w3.org/2000/svg">
                            <path d="M22.666 26.6667H29.3327V24C29.3327 21.7909 27.5418 20 25.3327 20C24.0585 20 22.9235 20.5957 22.191 21.5238M22.666 26.6667H9.33268M22.666 26.6667V24C22.666 23.125 22.4975 22.2894 22.191 21.5238M9.33268 26.6667H2.66602V24C2.66602 21.7909 4.45688 20 6.66602 20C7.94016 20 9.07516 20.5957 9.80768 21.5238M9.33268 26.6667V24C9.33268 23.125 9.50125 22.2894 9.80768 21.5238M9.80768 21.5238C10.7907 19.068 13.1924 17.3333 15.9993 17.3333C18.8063 17.3333 21.208 19.068 22.191 21.5238M19.9993 9.33333C19.9993 11.5425 18.2085 13.3333 15.9993 13.3333C13.7902 13.3333 11.9993 11.5425 11.9993 9.33333C11.9993 7.12419 13.7902 5.33333 15.9993 5.33333C18.2085 5.33333 19.9993 7.12419 19.9993 9.33333ZM27.9993 13.3333C27.9993 14.8061 26.8054 16 25.3327 16C23.8599 16 22.666 14.8061 22.666 13.3333C22.666 11.8606 23.8599 10.6667 25.3327 10.6667C26.8054 10.6667 27.9993 11.8606 27.9993 13.3333ZM9.33268 13.3333C9.33268 14.8061 8.13878 16 6.66602 16C5.19326 16 3.99935 14.8061 3.99935 13.3333C3.99935 11.8606 5.19326 10.6667 6.66602 10.6667C8.13878 10.6667 9.33268 11.8606 9.33268 13.3333Z" stroke="#323232" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
                        </svg>
                    </span>
                    {isOpen && (<span>Clientes</span>)}
                </NavLink>
            </li>
            <li>
                <NavLink to={"/companies"} className={event => `flex ${isOpen ? '' : 'justify-center'} items-center space-x-3 text-neutral-900 p-2 rounded-md font-medium hover:bg-violet-100 focus:shadow-outline cursor-pointer mb-2 ${event.isActive ? 'bg-violet-100' : ''}`}>
                    <span className="text-gray-600 ml-0.5">
                        <svg width="23" height="27" viewBox="0 0 23 27" fill="none" xmlns="http://www.w3.org/2000/svg">
                            <path d="M8.08211 15.5429C7.69158 15.1524 7.05842 15.1524 6.66789 15.5429C6.27737 15.9334 6.27737 16.5666 6.66789 16.9571L8.08211 15.5429ZM10.125 19L9.41789 19.7071C9.80842 20.0976 10.4416 20.0976 10.8321 19.7071L10.125 19ZM16.3321 14.2071C16.7226 13.8166 16.7226 13.1834 16.3321 12.7929C15.9416 12.4024 15.3084 12.4024 14.9179 12.7929L16.3321 14.2071ZM20.125 6.625V23.125H22.125V6.625H20.125ZM18.375 24.875H4.625V26.875H18.375V24.875ZM2.875 23.125V6.625H0.875V23.125H2.875ZM4.625 4.875H7.375V2.875H4.625V4.875ZM15.625 4.875H18.375V2.875H15.625V4.875ZM4.625 24.875C3.6585 24.875 2.875 24.0915 2.875 23.125H0.875C0.875 25.1961 2.55393 26.875 4.625 26.875V24.875ZM20.125 23.125C20.125 24.0915 19.3415 24.875 18.375 24.875V26.875C20.4461 26.875 22.125 25.1961 22.125 23.125H20.125ZM22.125 6.625C22.125 4.55393 20.4461 2.875 18.375 2.875V4.875C19.3415 4.875 20.125 5.6585 20.125 6.625H22.125ZM2.875 6.625C2.875 5.6585 3.6585 4.875 4.625 4.875V2.875C2.55393 2.875 0.875 4.55393 0.875 6.625H2.875ZM6.66789 16.9571L9.41789 19.7071L10.8321 18.2929L8.08211 15.5429L6.66789 16.9571ZM10.8321 19.7071L16.3321 14.2071L14.9179 12.7929L9.41789 18.2929L10.8321 19.7071ZM10.125 2.125H12.875V0.125H10.125V2.125ZM12.875 5.625H10.125V7.625H12.875V5.625ZM10.125 5.625C9.1585 5.625 8.375 4.8415 8.375 3.875H6.375C6.375 5.94607 8.05393 7.625 10.125 7.625V5.625ZM14.625 3.875C14.625 4.8415 13.8415 5.625 12.875 5.625V7.625C14.9461 7.625 16.625 5.94607 16.625 3.875H14.625ZM12.875 2.125C13.8415 2.125 14.625 2.9085 14.625 3.875H16.625C16.625 1.80393 14.9461 0.125 12.875 0.125V2.125ZM10.125 0.125C8.05393 0.125 6.375 1.80393 6.375 3.875H8.375C8.375 2.9085 9.1585 2.125 10.125 2.125V0.125Z" fill="#323232" />
                        </svg>
                    </span>
                    {isOpen && (<span>Seguradoras</span>)}
                </NavLink>
            </li>
            {
                isAuthenticated() && isAllowedByRole(['admin']) && (
                    <li>
                        <NavLink to={"/analysis"} className={event => `flex ${isOpen ? '' : 'justify-center'} items-center space-x-3 text-neutral-900 p-2 rounded-md font-medium hover:bg-violet-100 focus:shadow-outline cursor-pointer mb-2 ${event.isActive ? 'bg-violet-100' : ''}`}>
                            <span className="text-gray-600 ml-0.5">
                                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" className="w-6 h-6">
                                    <path strokeLinecap="round" strokeLinejoin="round" d="M3.75 3v11.25A2.25 2.25 0 006 16.5h2.25M3.75 3h-1.5m1.5 0h16.5m0 0h1.5m-1.5 0v11.25A2.25 2.25 0 0118 16.5h-2.25m-7.5 0h7.5m-7.5 0l-1 3m8.5-3l1 3m0 0l.5 1.5m-.5-1.5h-9.5m0 0l-.5 1.5M9 11.25v1.5M12 9v3.75m3-6v6" />
                                </svg>
                            </span>
                            {isOpen && (<span>Matriz de Risco</span>)}
                        </NavLink>
                    </li>
                )
            }
        </ul>
        <div className={`${isOpen ? '' : 'justify-center'} text-lg w-full p-4 mr-2 flex items-center cursor-pointer`} onClick={() => logout()}>
            <span>
                <svg width="32" height="32" viewBox="0 0 32 32" fill="none" xmlns="http://www.w3.org/2000/svg">
                    <path d="M22.6667 21.3333L28 16M28 16L22.6667 10.6667M28 16L9.33333 16M17.3333 21.3333V22.6667C17.3333 24.8758 15.5425 26.6667 13.3333 26.6667H8C5.79086 26.6667 4 24.8758 4 22.6667V9.33334C4 7.1242 5.79086 5.33334 8 5.33334H13.3333C15.5425 5.33334 17.3333 7.1242 17.3333 9.33334V10.6667" stroke="#DC2626" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
                </svg>
            </span>
            {isOpen && (<span className='font-bold ml-2 text-rose-600'>Sair</span>)}
        </div>
    </aside>
}

export default Aside;
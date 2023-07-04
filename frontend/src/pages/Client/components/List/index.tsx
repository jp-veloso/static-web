import { format } from "date-fns";
import { ptBR } from "date-fns/locale";
import { useCallback, useEffect, useState } from "react";
import ReactPaginate from "react-paginate";
import { useNavigate, useOutletContext } from "react-router-dom";
import { useShowModal } from "../..";
import Modal from "../../../../core/Modal";
import { makePrivateRequest } from "../../../../core/script";
import { PagedResponse, ClientModel } from '../../../../core/types';

type Props = {
    model: ClientModel
}

const List = () => {
    const { onModalChange } = useShowModal()
    const [activePage, setActivePage] = useState(1)
    const [filter, setFilter] = useState('')
    const [response, setResponse] = useState<PagedResponse>()

    const hidden = () => {
        return <div className="hidden"></div>
    }

    const getClients = useCallback(() => {
        const params = {
            page: activePage,
            sort: 'createdAt desc',
            filter,
            size: 5
        }

        makePrivateRequest({ url: "/clients", params })
            .then(r => setResponse(r.data))

    }, [filter, activePage])

    useEffect(() => {
        getClients()
    }, [getClients])

    return <div className="h-[calc(100vh-4rem)] overflow-y-scroll scrollbar">
        <div className="m-2 text-right">
            <button onClick={() => onModalChange({value: "LIST", show: true})} className="bg-neutral-300 p-2 rounded-md font-bold text-neutral-500 hover:bg-neutral-200">Cadastrar novo cliente</button>
        </div>
        <div className="text-center">
            <h1 className="font-bold text-neutral-900 text-5xl mb-4">Pesquisar por cliente</h1>
            <p className="text-neutral-400 text-2xl font-medium mb-4">Basta inserir o nome ou o CNPJ do cliente desejado que <br /> automaticamente irá retornar a sua busca!</p>
            <input className="bg-white h-12 rounded-md border text-neutral-800 border-2 w-96 mb-4 focus:outline-none p-4" placeholder="Pesquisa por nome ou cnpj" value={filter} onChange={event => setFilter(event.target.value)}></input>
        </div>
        <div className="flex flex-col items-center px-16">
            {response && response?.data.map(item => (<CardList key={item.id} model={item} />))}
        </div>
        {response && (
            <div>
                <ReactPaginate
                    pageCount={response.totalPages}
                    pageRangeDisplayed={5}
                    marginPagesDisplayed={1}
                    onPageChange={item => setActivePage(item.selected + 1)}
                    nextLabel={hidden()}
                    previousLabel={hidden()}
                    containerClassName="flex justify-center mt-2 mb-4"
                    pageLinkClassName="m-2 p-2 font-bold text-neutral-500 text-lg"
                    breakClassName="font-bold text-neutral-500 text-lg"
                    activeLinkClassName="border-b-2 border-violet-600 text-violet-600"
                />
            </div>
        )}
    </div>
}

const CardList = ({ model }: Props) => {
    const navigation = useNavigate()

    return <div className="rounded-md border bg-white flex h-20 w-full items-center justify-between p-4 my-2">
        <div className="basis-5/12 mr-2">
            <p className="text-base text-neutral-300 font-medium">Nome</p>
            <div className="font-bold text-neutral-900 overflow-hidden text-ellipsis w-auto h-6">{model.name}</div>
        </div>
        <div className="basis-3/12">
            <p className="text-base text-neutral-300 font-medium">CNPJ</p>
            <p className="font-bold text-neutral-900">{model.cnpj}</p>
        </div>
        <div className="basis-2/12">
            <p className="text-base text-neutral-300 font-medium">Registrado em</p>
            <p className="font-bold text-neutral-900">{format(new Date(model.createdAt), 'dd/MM/yyyy', { locale: ptBR })}</p>
        </div>
        <button className="basis-2/12 auth-button rounded-xl p-1 text-white h-10" onClick={() => navigation(`/clients/${model.id}`)}>Mais detalhes</button>
    </div>
}

export default List;
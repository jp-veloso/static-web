import { ClientInfo } from "../../../../core/types";
import _right from '../../../../assets/check-circle.svg'
import _wrong from '../../../../assets/x-circle.svg'
import _link from '../../../../assets/_link.svg'
import { format } from "date-fns";
import { ptBR } from "date-fns/locale";
import { useNavigate } from "react-router-dom";
import { ModalType } from "../..";

type Props = {
    client: ClientInfo
    modalFunc: (value: ModalType) => void;
}

const ClientCard = ({ client, modalFunc }: Props) => {
    const navigate = useNavigate()

    return <div className="h-fit fixed m-2 w-64">
        <div className="shadow-md rounded-2xl">
            <div className="bg-details-card bg-cover rounded-t-2xl text-center p-4">
                <label className="text-sm text-white">Razão social</label>
                <p className="text-base text-white">{client.name}</p>
            </div>
            <div>
                <div className="px-3 pt-3">
                    <label>CNPJ</label>
                    {client.pipeId ?
                        <a target="_blank" href={`https://app.pipe.run/contatos/empresas/#/visualizar/${client.pipeId}`} className="block font-bold text-base tracking-wider text-neutral-900 hover:text-violet-500 hover:cursor-pointer"><img src={_link} className='inline w-4'></img> {client.cnpj}</a> :
                        <p className="font-bold text-neutral-900">{client.cnpj}</p>
                    }
                </div>
                <div className="py-3 px-3">
                    <label>Carteira</label>
                    <p className="font-bold text-neutral-900">{client.segment}</p>
                </div>
                <div className="px-3 pb-3">
                    <label>Data de registro</label>
                    <p className="font-bold text-neutral-900">{format(new Date(client.createdAt), 'dd/MM/yyyy', { locale: ptBR })}</p>
                </div>
                <div className="px-3 pb-1">
                    <img src={client.isClient ? _right : _wrong} className='inline mb-[4px]'></img>
                    <span> Cliente</span>
                </div>
                <div className="px-3 pb-1">
                    <img src={client.active ? _right : _wrong} className='inline mb-[4px]'></img>
                    <span> Comprou recentemente</span>
                </div>
            </div>

        </div>
        <div className="flex justify-center items-center flex-col mb-2">
            <button onClick={() => modalFunc({ value: "INSURER", show: true, uniqueId: client.id })} className="rounded-xl p-2 text-white mt-3 bg-violet-300 w-full hover:bg-violet-200">Gerar carta de nomeação</button>
            <button className="rounded-xl p-2 text-white mt-3 auth-button w-full" onClick={() => navigate(`/clients/${client.id}/proposals`)}>Calcular propostas</button>
            <button onClick={() => modalFunc({ value: "CALCULATOR", show: true })} className="rounded-xl p-2 text-white mt-3 bg-violet-300 w-full hover:bg-violet-200">Descobrir taxa</button>
        </div>
    </div>
}

export default ClientCard;
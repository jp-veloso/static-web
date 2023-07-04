import _right from '../../../../assets/check-circle.svg'
import _wrong from '../../../../assets/x-circle.svg'
import _link from '../../../../assets/_link.svg'
import { useCallback, useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { makePrivateRequest } from "../../../../core/script";
import { ClientInfo } from "../../../../core/types";
import EnrollmentContainer from "../EnrollmentContainer";
import { useShowModal } from "../..";
import ClientCard from "../ClientCard";

const Details = () => {
    const { id } = useParams()
    const { onModalChange } = useShowModal()
    const [client, setClient] = useState<ClientInfo>()

    const getClient = useCallback(() => {
        makePrivateRequest({ url: `/clients/${id}` })
            .then(response => { setClient(response.data) })
    }, [])

    useEffect(() => {
        getClient()
    }, [getClient])


    return <div className="h-[calc(100vh-4rem)] overflow-y-scroll scrollbar flex relative">
        <div className="w-96"></div>
        {client && <ClientCard client={client} modalFunc={onModalChange}/>}
        <EnrollmentContainer clientId={Number(id)} modalFunc={onModalChange} />
    </div>
}

export default Details;